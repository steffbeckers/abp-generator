using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MyCompany.MyProduct.EntityFrameworkCore
{
    // This class is needed for EF Core console commands like Add-Migration and Update-Database commands.
    public class MyProductDbContextFactory : IDesignTimeDbContextFactory<MyProductDbContext>
    {
        public MyProductDbContext CreateDbContext(string[] args)
        {
            MyProductEfCoreEntityExtensionMappings.Configure();

            IConfigurationRoot configuration = BuildConfiguration();

            DbContextOptionsBuilder<MyProductDbContext> builder = new DbContextOptionsBuilder<MyProductDbContext>()
                .UseSqlServer(configuration.GetConnectionString("Default"));

            return new MyProductDbContext(builder.Options);
        }

        private static IConfigurationRoot BuildConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../MyCompany.MyProduct.DbMigrator/"))
                .AddJsonFile("appsettings.json", optional: false);

            return builder.Build();
        }
    }
}