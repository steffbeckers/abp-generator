using Volo.Abp.Application.Dtos;

namespace MyCompany.MyProduct.Shared
{
    public class PagedListInputDto : PagedResultRequestDto
    {
        public string FilterText { get; set; }
    }
}