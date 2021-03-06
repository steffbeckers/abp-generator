using MyCompany.MyProduct.Localization;
using Volo.Abp.AuditLogging;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.IdentityServer;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;
using Volo.Abp.Validation.Localization;
using Volo.Abp.VirtualFileSystem;

namespace MyCompany.MyProduct
{
    [DependsOn(
        typeof(AbpAuditLoggingDomainSharedModule),
        typeof(AbpBackgroundJobsDomainSharedModule),
        typeof(AbpFeatureManagementDomainSharedModule),
        typeof(AbpIdentityDomainSharedModule),
        typeof(AbpIdentityServerDomainSharedModule),
        typeof(AbpPermissionManagementDomainSharedModule),
        typeof(AbpSettingManagementDomainSharedModule),
        typeof(AbpTenantManagementDomainSharedModule))]
    public class MyProductDomainSharedModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.AddEmbedded<MyProductDomainSharedModule>();
            });

            Configure<AbpLocalizationOptions>(options =>
            {
                options.Resources
                    .Add<MyProductResource>("en")
                    .AddBaseTypes(typeof(AbpValidationResource))
                    .AddVirtualJson("/Localization/MyProduct");

                options.DefaultResourceType = typeof(MyProductResource);
            });

            // You can map your business exception error code prefixes here. Example:
            Configure<AbpExceptionLocalizationOptions>(options =>
            {
                options.MapCodeNamespace(nameof(MyProduct), typeof(MyProductResource));
                options.MapCodeNamespace(nameof(MyProductDomainErrorCodes.Samples), typeof(MyProductResource));
            });
        }

        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            MyProductGlobalFeatureConfigurator.Configure();
            MyProductModuleExtensionConfigurator.Configure();
        }
    }
}