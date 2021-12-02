using Volo.Abp.Application.Dtos;

namespace MyCompany.MyProduct.Shared
{
    public class LookupDto<TKey> : EntityDto<TKey>
    {
        public string Name { get; set; }
    }
}