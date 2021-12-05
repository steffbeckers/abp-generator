using System.Text.RegularExpressions;

namespace SteffBeckers.Abp.Generator.Templates;

public class SnippetTemplateContext
{
    public string? OutputPath { get; set; }

    public string? RegexPattern { get; set; }

    public string? RegexReplacement { get; set; }

    public RegexOptions RegexOptions { get; set; } = RegexOptions.None;

    public bool RunForEachEntity { get; set; } = false;
}