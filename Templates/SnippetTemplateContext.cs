namespace SteffBeckers.Abp.Generator.Templates;

public class SnippetTemplateContext
{
    public string? OutputPath { get; set; }

    public string? Pattern { get; set; }

    public string? Replacement { get; set; }

    public bool RunForEachEntity { get; set; } = false;
}