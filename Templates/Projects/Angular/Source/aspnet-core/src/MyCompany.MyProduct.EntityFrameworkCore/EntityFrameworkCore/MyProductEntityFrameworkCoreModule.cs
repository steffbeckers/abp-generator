using Microsoft.Extensions.DependencyInjection;
using MyCompany.MyProduct.Samples;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.DependencyInjection;
using Volo.Abp.EntityFrameworkCore.SqlServer;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.IdentityServer.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace MyCompany.MyProduct.EntityFrameworkCore
{
    [DependsOn(
        typeof(AbpAuditLoggingEntityFrameworkCoreModule),
        typeof(AbpBackgroundJobsEntityFrameworkCoreModule),
        typeof(AbpEntityFrameworkCoreSqlServerModule),
        typeof(AbpFeatureManagementEntityFrameworkCoreModule),
        typeof(AbpIdentityEntityFrameworkCoreModule),
        typeof(AbpIdentityServerEntityFrameworkCoreModule),
        typeof(AbpPermissionManagementEntityFrameworkCoreModule),
        typeof(AbpSettingManagementEntityFrameworkCoreModule),
        typeof(AbpTenantManagementEntityFrameworkCoreModule),
        typeof(MyProductDomainModule))]
    public class MyProductEntityFrameworkCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddAbpDbContext<MyProductDbContext>(options =>
            {
                options.AddDefaultRepositories();

                // Add custom repositories for your aggregate root entities to options. Example:
                options.AddRepository<Sample, EfCoreSampleRepository>();
            });

            Configure<AbpDbContextOptions>(options =>
            {
                options.UseSqlServer();
            });

            Configure<AbpEntityOptions>(options =>
            {
            });
        }

        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            MyProductEfCoreEntityExtensionMappings.Configure();
        }
    }
}