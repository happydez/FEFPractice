using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace AutoLot.Dal.EfStructures
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(GetDbConnectionString());

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private string GetDbConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(@"C:\FEFPractice\")
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            return config["connectionString"];
        }
    }
}
