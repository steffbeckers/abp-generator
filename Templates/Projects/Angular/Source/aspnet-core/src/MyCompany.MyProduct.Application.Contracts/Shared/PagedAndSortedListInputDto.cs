using Volo.Abp.Application.Dtos;

namespace MyCompany.MyProduct.Shared
{
    public class PagedAndSortedListInputDto : PagedAndSortedResultRequestDto
    {
        public string FilterText { get; set; }
    }
}