using Localization.Resources.AbpUi;
using MyCompany.MyProduct.Localization;
using Volo.Abp.Account;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.HttpApi;
using Volo.Abp.SettingManagement;
using Volo.Abp.TenantManagement;

namespace MyCompany.MyProduct
{
    [DependsOn(
        typeof(AbpAccountHttpApiModule),
        typeof(AbpFeatureManagementHttpApiModule),
        typeof(AbpIdentityHttpApiModule),
        typeof(AbpPermissionManagementHttpApiModule),
        typeof(AbpSettingManagementHttpApiModule),
        typeof(AbpTenantManagementHttpApiModule),
        typeof(MyProductApplicationContractsModule))]
    public class MyProductHttpApiModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            ConfigureLocalization();
        }

        private void ConfigureLocalization()
        {
            Configure<AbpLocalizationOptions>(
                options => options.Resources.Get<MyProductResource>().AddBaseTypes(typeof(AbpUiResource)));
        }
    }
}