using MyCompany.MyProduct.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace MyCompany.MyProduct
{
    [DependsOn(
        typeof(MyProductEntityFrameworkCoreTestModule)
        )]
    public class MyProductDomainTestModule : AbpModule
    {

    }
}