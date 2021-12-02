using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyCompany.MyProduct.Data;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;

namespace MyCompany.MyProduct.DbMigrator
{
    public class DbMigratorHostedService : IHostedService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public DbMigratorHostedService(IConfiguration configuration, IHostApplicationLifetime hostApplicationLifetime)
        {
            _configuration = configuration;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (IAbpApplicationWithInternalServiceProvider application = AbpApplicationFactory.Create<MyProductDbMigratorModule>(
                options =>
                {
                    options.Services.ReplaceConfiguration(_configuration);
                    options.UseAutofac();
                    options.Services.AddLogging(c => c.AddSerilog());
                }))
            {
                application.Initialize();

                await application.ServiceProvider.GetRequiredService<MyProductDbMigrationService>().MigrateAsync();

                application.Shutdown();

                _hostApplicationLifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}