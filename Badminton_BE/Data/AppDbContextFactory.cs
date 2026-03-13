using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Badminton_BE.Data
{
    // Design-time factory for EF Core tools (migrations, update-database, etc.)
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = "server=localhost;port=3306;database=BadmintonDb;user=root;password=Mun1401@";
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Use a fixed server version so EF tooling can work without a live database connection.
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));
            optionsBuilder.UseMySql(connectionString, serverVersion);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
