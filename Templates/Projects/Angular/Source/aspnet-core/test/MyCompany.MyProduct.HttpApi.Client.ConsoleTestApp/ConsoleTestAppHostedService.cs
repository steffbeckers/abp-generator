using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;

namespace MyCompany.MyProduct.HttpApi.Client.ConsoleTestApp
{
    public class ConsoleTestAppHostedService : IHostedService
    {
        private readonly IConfiguration _configuration;

        public ConsoleTestAppHostedService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (IAbpApplicationWithInternalServiceProvider application = AbpApplicationFactory.Create<MyProductConsoleApiClientModule>(
                options => options.Services.ReplaceConfiguration(_configuration)))
            {
                application.Initialize();

                ClientDemoService demo = application.ServiceProvider.GetRequiredService<ClientDemoService>();
                await demo.RunAsync();

                application.Shutdown();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}