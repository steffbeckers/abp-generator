using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.Threading.Tasks;

namespace MyCompany.MyProduct.DbMigrator
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(build => build.AddJsonFile("appsettings.secrets.json", optional: true))
                .ConfigureLogging((context, logging) => logging.ClearProviders())
                .ConfigureServices((hostContext, services) => services.AddHostedService<DbMigratorHostedService>());
        }

        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel
                .Information()
                .MinimumLevel
                .Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel
                .Override("Volo.Abp", LogEventLevel.Warning)
#if DEBUG
                .MinimumLevel
                .Override("MyCompany.MyProduct", LogEventLevel.Debug)
#else
                .MinimumLevel.Override("MyCompany.MyProduct", LogEventLevel.Information)
#endif

                .Enrich
                .FromLogContext()
                .WriteTo
                .Async(c => c.File("Logs/logs.txt"))
                .WriteTo
                .Async(c => c.Console())
                .CreateLogger();

            await CreateHostBuilder(args).RunConsoleAsync();
        }
    }
}