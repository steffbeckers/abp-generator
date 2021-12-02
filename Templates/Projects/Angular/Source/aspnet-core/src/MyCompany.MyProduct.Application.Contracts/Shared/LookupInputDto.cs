using System;

namespace MyCompany.MyProduct.Shared
{
    public class LookupInputDto : PagedListInputDto
    {
        public Guid? Id { get; set; }
    }
}