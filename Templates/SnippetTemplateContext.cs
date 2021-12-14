using System.Text.RegularExpressions;

namespace SteffBeckers.Abp.Generator.Templates;

public class SnippetTemplateContext
{
    public string? OutputPath { get; set; }

    public RegexOptions RegexOptions { get; set; } = RegexOptions.None;

    public string? RegexPattern { get; set; }

    public string? RegexReplacement { get; set; }

    public bool RunForEachEntity { get; set; } = false;
}