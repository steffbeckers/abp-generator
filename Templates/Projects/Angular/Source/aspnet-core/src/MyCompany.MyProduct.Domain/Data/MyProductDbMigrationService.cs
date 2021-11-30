using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace MyCompany.MyProduct.Data
{
    public class MyProductDbMigrationService : ITransientDependency
    {
        private readonly ICurrentTenant _currentTenant;
        private readonly IDataSeeder _dataSeeder;
        private readonly IEnumerable<IMyProductDbSchemaMigrator> _dbSchemaMigrators;
        private readonly ILogger<MyProductDbMigrationService> _logger;
        private readonly ITenantRepository _tenantRepository;

        public MyProductDbMigrationService(
            ICurrentTenant currentTenant,
            IDataSeeder dataSeeder,
            IEnumerable<IMyProductDbSchemaMigrator> dbSchemaMigrators,
            ITenantRepository tenantRepository)
        {
            _currentTenant = currentTenant;
            _dataSeeder = dataSeeder;
            _dbSchemaMigrators = dbSchemaMigrators;
            _logger = NullLogger<MyProductDbMigrationService>.Instance;
            _tenantRepository = tenantRepository;
        }

        private void AddInitialMigration()
        {
            _logger.LogInformation("Creating initial migration...");

            string argumentPrefix;
            string fileName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                argumentPrefix = "-c";
                fileName = "/bin/bash";
            }
            else
            {
                argumentPrefix = "/C";
                fileName = "cmd.exe";
            }

            ProcessStartInfo procStartInfo = new ProcessStartInfo(
                fileName,
                $"{argumentPrefix} \"abp create-migration-and-run-migrator \"{GetEntityFrameworkCoreProjectFolderPath()}\"\"");

            try
            {
                Process.Start(procStartInfo);
            }
            catch (Exception)
            {
                throw new Exception("Couldn't run ABP CLI...");
            }
        }

        private bool AddInitialMigrationIfNotExist()
        {
            try
            {
                if (!DbMigrationsProjectExists())
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            try
            {
                if (!MigrationsFolderExists())
                {
                    AddInitialMigration();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning("Couldn't determinate if any migrations exist : " + e.Message);
                return false;
            }
        }

        private bool DbMigrationsProjectExists()
        {
            string dbMigrationsProjectFolder = GetEntityFrameworkCoreProjectFolderPath();

            return dbMigrationsProjectFolder != null;
        }

        private string GetEntityFrameworkCoreProjectFolderPath()
        {
            string slnDirectoryPath = GetSolutionDirectoryPath();

            if (slnDirectoryPath == null)
            {
                throw new Exception("Solution folder not found!");
            }

            string srcDirectoryPath = Path.Combine(slnDirectoryPath, "src");

            return Directory.GetDirectories(srcDirectoryPath).FirstOrDefault(d => d.EndsWith(".EntityFrameworkCore"));
        }

        private string GetSolutionDirectoryPath()
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (Directory.GetParent(currentDirectory.FullName) != null)
            {
                currentDirectory = Directory.GetParent(currentDirectory.FullName);

                if (Directory.GetFiles(currentDirectory.FullName).FirstOrDefault(f => f.EndsWith(".sln")) != null)
                {
                    return currentDirectory.FullName;
                }
            }

            return null;
        }

        public async Task MigrateAsync()
        {
            bool initialMigrationAdded = AddInitialMigrationIfNotExist();

            if (initialMigrationAdded)
            {
                return;
            }

            _logger.LogInformation("Started database migrations...");

            await MigrateDatabaseSchemaAsync();
            await SeedDataAsync();

            _logger.LogInformation($"Successfully completed host database migrations.");

            List<Tenant> tenants = await _tenantRepository.GetListAsync(includeDetails: true);

            HashSet<string> migratedDatabaseSchemas = new HashSet<string>();
            foreach (Tenant tenant in tenants)
            {
                using (_currentTenant.Change(tenant.Id))
                {
                    if (tenant.ConnectionStrings.Any())
                    {
                        List<string> tenantConnectionStrings = tenant.ConnectionStrings.Select(x => x.Value).ToList();

                        if (!migratedDatabaseSchemas.IsSupersetOf(tenantConnectionStrings))
                        {
                            await MigrateDatabaseSchemaAsync(tenant);

                            migratedDatabaseSchemas.AddIfNotContains(tenantConnectionStrings);
                        }
                    }

                    await SeedDataAsync(tenant);
                }

                _logger.LogInformation($"Successfully completed {tenant.Name} tenant database migrations.");
            }

            _logger.LogInformation("Successfully completed all database migrations.");
            _logger.LogInformation("You can safely end this process...");
        }

        private async Task MigrateDatabaseSchemaAsync(Tenant tenant = null)
        {
            _logger.LogInformation(
                $"Migrating schema for {(tenant == null ? "host" : tenant.Name + " tenant")} database...");

            foreach (IMyProductDbSchemaMigrator migrator in _dbSchemaMigrators)
            {
                await migrator.MigrateAsync();
            }
        }

        private bool MigrationsFolderExists()
        {
            string dbMigrationsProjectFolder = GetEntityFrameworkCoreProjectFolderPath();

            return Directory.Exists(Path.Combine(dbMigrationsProjectFolder, "Migrations"));
        }

        private async Task SeedDataAsync(Tenant tenant = null)
        {
            _logger.LogInformation($"Executing {(tenant == null ? "host" : tenant.Name + " tenant")} database seed...");

            await _dataSeeder.SeedAsync(
                new DataSeedContext(tenant?.Id)
                .WithProperty(
                    IdentityDataSeedContributor.AdminEmailPropertyName,
                    IdentityDataSeedContributor.AdminEmailDefaultValue)
                    .WithProperty(
                        IdentityDataSeedContributor.AdminPasswordPropertyName,
                        IdentityDataSeedContributor.AdminPasswordDefaultValue));
        }
    }
}
