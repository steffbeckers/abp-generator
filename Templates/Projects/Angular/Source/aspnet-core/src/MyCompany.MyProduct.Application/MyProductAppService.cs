using System;
using System.Collections.Generic;
using System.Text;
using MyCompany.MyProduct.Localization;
using Volo.Abp.Application.Services;

namespace MyCompany.MyProduct
{
    /* Inherit your application services from this class.
     */
    public abstract class MyProductAppService : ApplicationService
    {
        protected MyProductAppService()
        {
            LocalizationResource = typeof(MyProductResource);
        }
    }
}
