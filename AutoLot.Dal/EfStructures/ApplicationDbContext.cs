using System;
using System.Collections;
using System.Collections.Generic;
using AutoLot.Models.Entities;
using AutoLot.Models.Entities.Owned;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using AutoLot.Dal.Exceptions;
using AutoLot.Models.ViewModels;

namespace AutoLot.Dal.EfStructures
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            ChangeTracker.StateChanged += ChangeTracker_StateChanged;
            ChangeTracker.Tracked += ChangeTracker_Tracked;

            base.SavingChanges += (sender, args) =>
            {
                Console.WriteLine($"Saving changes for {((ApplicationDbContext)sender)!.Database!.GetConnectionString()}");
            };

            base.SavedChanges += (sender, args) =>
            {
                Console.WriteLine($"Saved {args!.EntitiesSavedCount} changes for {((ApplicationDbContext)sender)!.Database!.GetConnectionString()}");
            };
            base.SaveChangesFailed += (sender, args) =>
            {
                Console.WriteLine($"An exception occurred! {args.Exception.Message} entities");
            };
        }

        private void ChangeTracker_StateChanged(object? sender, EntityStateChangedEventArgs e)
        {
            if (e.Entry.Entity is not Car c)
            {
                return;
            }

            var action = string.Empty;
            Console.WriteLine($"Car {c.PetName} was {e.OldState} before the state changed to {e.NewState}");
            
            switch (e.NewState)
            {
                case EntityState.Unchanged:
                    action = e.OldState switch
                    {
                        EntityState.Added => "Added",
                        EntityState.Modified => "Edited",
                        _ => action
                    };

                    Console.WriteLine($"The object was {action}");
                    break;
            }
        }

        private void ChangeTracker_Tracked(object? sender, EntityTrackedEventArgs e)
        {
            var source = (e.FromQuery) ? "Database" : "Code";

            if (e.Entry.Entity is Car c)
            {
                Console.WriteLine($"Car entry {c.PetName} was added from {source}");
            }
        }

        public DbSet<CreditRisk>? CreditRisks { get; set; } = null!;
        public DbSet<Customer>? Customers { get; set; } = null!;
        public DbSet<Car>? Cars { get; set; } = null!;
        public DbSet<Make>? Makes { get; set; } = null!;
        public DbSet<Order>? Orders { get; set; } = null!;
        public DbSet<CustomerOrderViewModel>? CustomerOrderViewModel { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerOrderViewModel>(entity =>
            {
                entity.HasNoKey().ToView("CustomerOrderView", "dbo");
            });

            modelBuilder.Entity<SeriLogEntry>(entity =>
            {
                entity.Property(e => e.Properties).HasColumnName("Xml");
                entity.Property(e => e.TimeStamp).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<CreditRisk>(entity =>
            {
                entity.OwnsOne(owns => owns.PersonalInformation, navigation =>
                {
                    navigation.Property<string>(nameof(Person.FirstName))
                        .HasColumnName(nameof(Person.FirstName))
                        .HasColumnType("nvarchar(50)");

                    navigation.Property<string>(nameof(Person.LastName))
                        .HasColumnName(nameof(Person.LastName))
                        .HasColumnType("nvarchar(50)");

                    navigation.Property(p => p.FullName)
                        .HasColumnName(nameof(Person.FullName))
                        .HasComputedColumnSql("[LastName] + ', ' + [FirstName]");
                });

                entity.HasOne(d => d.CustomerNavigation)
                    .WithMany(p => p.CreditRisks)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_CreditRisks_Customers");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.OwnsOne(owns => owns.PersonalInformation, navigation =>
                {
                    navigation.Property(p => p.FirstName).HasColumnName(nameof(Person.FirstName));
                    navigation.Property(p => p.LastName).HasColumnName(nameof(Person.LastName));
                    navigation.Property(p => p.FullName).HasColumnName(nameof(Person.FullName)).HasComputedColumnSql("[LastName] + ', ' + [FirstName]");
                });
            });

            modelBuilder.Entity<Car>(entity =>
            {
                entity.HasQueryFilter(c => c.IsDrivable);

                entity.Property(p => p.IsDrivable).HasField("_isDrivable").HasDefaultValue(true);

                entity.HasOne(d => d.MakeNavigation)
                    .WithMany(p => p.Cars)
                    .HasForeignKey(d => d.MakeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Make_Inventory");
            });

            modelBuilder.Entity<Make>(entity =>
            {
                entity.HasMany(e => e.Cars)
                .WithOne(c => c.MakeNavigation)
                .HasForeignKey(c => c.MakeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Make_Inventory");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasQueryFilter(e => e.CarNavigation!.IsDrivable);

                entity.HasOne(d => d.CarNavigation)
                    .WithMany(p => p!.Orders)
                    .HasForeignKey(d => d.CarId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Orders_Inventory");

                entity.HasOne(d => d.CustomerNavigation)
                    .WithMany(p => p!.Orders)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Orders_Customers");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                //A concurrency error occurred
                //Should log and handle intelligently
                throw new CustomConcurrencyException("A concurrency error happened.", ex);
            }
            catch (RetryLimitExceededException ex)
            {
                //DbResiliency retry limit exceeded
                //Should log and handle intelligently
                throw new CustomRetryLimitExceededException("There is a problem with SQl Server.", ex);
            }
            catch (DbUpdateException ex)
            {
                //Should log and handle intelligently
                throw new CustomDbUpdateException("An error occurred updating the database", ex);
            }
            catch (Exception ex)
            {
                //Should log and handle intelligently
                throw new CustomException("An error occurred updating the database", ex);
            }
        }
    }
}
