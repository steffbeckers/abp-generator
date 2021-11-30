using MyCompany.MyProduct.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace MyCompany.MyProduct.Controllers
{
    // Inherit your controllers from this class.
    public abstract class MyProductController : AbpController
    {
        protected MyProductController()
        {
            LocalizationResource = typeof(MyProductResource);
        }
    }
}