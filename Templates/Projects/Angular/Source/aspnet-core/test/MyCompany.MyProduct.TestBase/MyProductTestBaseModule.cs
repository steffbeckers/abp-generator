using Microsoft.Extensions.DependencyInjection;
using System;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Data;
using Volo.Abp.IdentityServer;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace MyCompany.MyProduct
{
    [DependsOn(
        typeof(AbpAuthorizationModule),
        typeof(AbpAutofacModule),
        typeof(AbpTestBaseModule),
        typeof(MyProductDomainModule))]
    public class MyProductTestBaseModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);

            context.Services.AddAlwaysAllowAuthorization();
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            SeedTestData(context);
        }

        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            PreConfigure<AbpIdentityServerBuilderOptions>(options => options.AddDeveloperSigningCredential = false);

            PreConfigure<IIdentityServerBuilder>(
                identityServerBuilder => identityServerBuilder.AddDeveloperSigningCredential(
                    false, Guid.NewGuid().ToString()));
        }

        private static void SeedTestData(ApplicationInitializationContext context)
        {
            AsyncHelper.RunSync(
                async () =>
                {
                    using (IServiceScope scope = context.ServiceProvider.CreateScope())
                    {
                        await scope.ServiceProvider.GetRequiredService<IDataSeeder>().SeedAsync();
                    }
                });
        }
    }
}