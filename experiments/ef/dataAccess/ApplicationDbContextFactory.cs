using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace dataAccess
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext Create()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var basePath = AppContext.BaseDirectory;

            return Create(basePath, environmentName);
        }

        public ApplicationDbContext CreateDbContext(string[] args)
        {
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"ASP.NET Environment Variable: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
            return Create(Directory.GetCurrentDirectory(), Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
        }

        private ApplicationDbContext Create(string basePath, string environmentName)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            var connstr = config.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(connstr))
            {
                throw new InvalidOperationException("Could not find a connection string named 'default'");
            }
            else
            {
                return Create(connstr);
            }
        }

        private ApplicationDbContext Create(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"{nameof(connectionString)} is null or empty.", nameof(connectionString));
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        //public ApplicationDbContext CreateDbContext(string[] args)
        //{
        //    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        //    optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Application.db;Trusted_Connection=True;ConnectRetryCount=0");

        //    return new ApplicationDbContext(optionsBuilder.Options);
        //}
    }
}
