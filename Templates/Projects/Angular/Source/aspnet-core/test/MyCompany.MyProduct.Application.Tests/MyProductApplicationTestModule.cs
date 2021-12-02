using Volo.Abp.Modularity;

namespace MyCompany.MyProduct
{
    [DependsOn(typeof(MyProductApplicationModule), typeof(MyProductDomainTestModule))]
    public class MyProductApplicationTestModule : AbpModule
    {
    }
}