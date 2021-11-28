using System.ComponentModel.DataAnnotations;

namespace SteffBeckers.Abp.Generator.Templates;

    public class ProjectTemplateGenerateInputDto
    {
    [Required]
    public string TemplateName { get; set; } = string.Empty;
    }
