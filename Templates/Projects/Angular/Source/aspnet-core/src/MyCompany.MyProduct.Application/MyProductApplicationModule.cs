using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace MyCompany.MyProduct
{
    [DependsOn(
        typeof(AbpAccountApplicationModule),
        typeof(AbpFeatureManagementApplicationModule),
        typeof(AbpIdentityApplicationModule),
        typeof(AbpPermissionManagementApplicationModule),
        typeof(AbpSettingManagementApplicationModule),
        typeof(AbpTenantManagementApplicationModule),
        typeof(MyProductApplicationContractsModule),
        typeof(MyProductDomainModule))]
    public class MyProductApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options => options.AddMaps<MyProductApplicationModule>());
        }
    }
}
