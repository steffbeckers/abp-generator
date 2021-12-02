using FluentValidation;
using Microsoft.Extensions.Localization;
using MyCompany.MyProduct.Localization;
using System.ComponentModel.DataAnnotations;

namespace MyCompany.MyProduct.Samples
{
    public class SampleCreateInputDto
    {
        [StringLength(SampleConsts.DescriptionMaxLength)]
        public string Description { get; set; }

        [Required]
        [StringLength(SampleConsts.NameMaxLength)]
        public string Name { get; set; }
    }

    public class SampleCreateInputDtoValidator : AbstractValidator<SampleCreateInputDto>
    {
        public SampleCreateInputDtoValidator(IStringLocalizer<MyProductResource> stringLocalizer)
        {
        }
    }
}