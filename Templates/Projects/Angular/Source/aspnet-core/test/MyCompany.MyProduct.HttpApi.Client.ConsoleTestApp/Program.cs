using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace MyCompany.MyProduct.HttpApi.Client.ConsoleTestApp
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(build => build.AddJsonFile("appsettings.secrets.json", optional: true))
                .ConfigureServices((hostContext, services) => services.AddHostedService<ConsoleTestAppHostedService>());
        }

        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).RunConsoleAsync();
        }
    }
}