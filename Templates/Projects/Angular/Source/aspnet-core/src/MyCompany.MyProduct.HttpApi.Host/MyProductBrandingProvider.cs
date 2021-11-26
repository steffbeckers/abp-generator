using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace MyCompany.MyProduct
{
    [Dependency(ReplaceServices = true)]
    public class MyProductBrandingProvider : DefaultBrandingProvider
    {
        public override string AppName => "MyProduct";
    }
}
