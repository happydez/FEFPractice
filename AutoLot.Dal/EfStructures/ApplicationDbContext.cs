using System;
using System.Collections;
using System.Collections.Generic;
using AutoLot.Models.Entities;
using AutoLot.Models.Entities.Owned;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AutoLot.Dal.EfStructures
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CreditRisk> CreditRisks { get; set; } = null!;
        public virtual DbSet<Customer> Customers { get; set; } = null!;
        public virtual DbSet<Car> Cars { get; set; } = null!;
        public virtual DbSet<Make> Makes { get; set; } = null!;
        public virtual DbSet<Order> Orders { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                entity.ToTable("Inventory");

                entity.HasIndex(e => e.MakeId, "IX_Inventory_MakeId");

                entity.Property(e => e.Color).HasMaxLength(50);

                entity.Property(e => e.PetName).HasMaxLength(50);

                entity.Property(e => e.TimeStamp)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.MakeNavigation)
                    .WithMany(p => p.Cars)
                    .HasForeignKey(d => d.MakeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Make_Inventory");
            });

            modelBuilder.Entity<Make>(entity =>
            {
                entity.Property(e => e.Name).HasMaxLength(50);

                entity.Property(e => e.TimeStamp)
                    .IsRowVersion()
                    .IsConcurrencyToken();
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasIndex(e => e.CarId, "IX_Orders_CarId");

                entity.HasIndex(e => new { e.CustomerId, e.CarId }, "IX_Orders_CustomerId_CarId")
                    .IsUnique();

                entity.Property(e => e.TimeStamp)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasOne(d => d.CarNavigation)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CarId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Orders_Inventory");

                entity.HasOne(d => d.CustomerNavigation)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.CustomerId)
                    .HasConstraintName("FK_Orders_Customers");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
