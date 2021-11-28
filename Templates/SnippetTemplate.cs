
namespace SteffBeckers.Abp.Generator.Templates;

    public class SnippetTemplate
    {
        public SnippetTemplateContext Context { get; set; } = new SnippetTemplateContext();

        public string? FullPath { get; set; }

        public string? Output { get; set; }

        public string? OutputPath { get; set; }
    }