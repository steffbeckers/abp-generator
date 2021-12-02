using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace MyCompany.MyProduct
{
    [DependsOn(
        typeof(AbpAccountHttpApiClientModule),
        typeof(AbpFeatureManagementHttpApiClientModule),
        typeof(AbpIdentityHttpApiClientModule),
        typeof(AbpPermissionManagementHttpApiClientModule),
        typeof(AbpSettingManagementHttpApiClientModule),
        typeof(AbpTenantManagementHttpApiClientModule),
        typeof(MyProductApplicationContractsModule))]
    public class MyProductHttpApiClientModule : AbpModule
    {
        public const string RemoteServiceName = nameof(MyProduct);

        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services
                .AddHttpClientProxies(typeof(MyProductApplicationContractsModule).Assembly, RemoteServiceName);
        }
    }
}