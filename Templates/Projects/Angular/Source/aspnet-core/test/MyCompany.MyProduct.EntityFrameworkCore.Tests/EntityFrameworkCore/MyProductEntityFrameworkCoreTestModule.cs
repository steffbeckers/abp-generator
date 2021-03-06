using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;

namespace MyCompany.MyProduct.EntityFrameworkCore
{
    [DependsOn(
        typeof(AbpEntityFrameworkCoreSqliteModule),
        typeof(MyProductEntityFrameworkCoreModule),
        typeof(MyProductTestBaseModule))]
    public class MyProductEntityFrameworkCoreTestModule : AbpModule
    {
        private SqliteConnection _sqliteConnection;

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            ConfigureInMemorySqlite(context.Services);
        }

        public override void OnApplicationShutdown(ApplicationShutdownContext context)
        {
            _sqliteConnection.Dispose();
        }

        private static SqliteConnection CreateDatabaseAndGetConnection()
        {
            SqliteConnection connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            DbContextOptions<MyProductDbContext> options = new DbContextOptionsBuilder<MyProductDbContext>()
                .UseSqlite(connection)
                .Options;

            using (MyProductDbContext context = new MyProductDbContext(options))
            {
                context.GetService<IRelationalDatabaseCreator>().CreateTables();
            }

            return connection;
        }

        private void ConfigureInMemorySqlite(IServiceCollection services)
        {
            _sqliteConnection = CreateDatabaseAndGetConnection();

            services.Configure<AbpDbContextOptions>(
                options => options.Configure(
                    context =>
                    {
                        context.DbContextOptions.UseSqlite(_sqliteConnection);
                    }));
        }
    }
}