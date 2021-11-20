using HandlebarsDotNet;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;
using SteffBeckers.Abp.Generator.Settings;

namespace SteffBeckers.Abp.Generator.Templates;

public class SnippetTemplateManager
{
    public readonly List<SnippetTemplate> Templates = new List<SnippetTemplate>();

    private readonly IHubContext<RealtimeHub> _realtimeHub;
    private readonly SettingManager _settingsManager;
    private readonly string _templateConfigDelimiter = "#-#-#";

    public SnippetTemplateManager(
        IHubContext<RealtimeHub> realtimeHub,
        SettingManager settingsManager)
    {
        _realtimeHub = realtimeHub;
        _settingsManager = settingsManager;
    }

    public async Task InitializeAsync()
    {
        if (!Directory.Exists(FileHelpers.UserBasedSnippetTemplatesPath))
        {
            Directory.CreateDirectory(FileHelpers.UserBasedSnippetTemplatesPath);
            FileHelpers.CopyFilesRecursively(FileHelpers.SnippetTemplatesPath, FileHelpers.UserBasedSnippetTemplatesPath);
        }

        await LoadTemplatesAsync();

        _settingsManager.Monitor.OnChange(async (settings) => await LoadTemplatesAsync());

        FileSystemWatcher? watcher = new FileSystemWatcher(FileHelpers.UserBasedTemplatesPath, "*.hbs");
        watcher.EnableRaisingEvents = true;
        watcher.Changed += async (file, eventArgs) => await LoadTemplateAsync(eventArgs.FullPath);
    }

    public async Task LoadTemplateAsync(string fullPath)
    {
        SnippetTemplate? template = Templates.FirstOrDefault(x => x.FullPath == fullPath);

        string outputPath = fullPath
            .Replace(FileHelpers.UserBasedSnippetTemplatesPath + Path.DirectorySeparatorChar, "")
            .Replace(".hbs", "");

        // TODO: Reflection based string replacement?
        outputPath = outputPath.Replace("{{Project.Name}}", _settingsManager.Settings.Context.Project.Name);
        outputPath = outputPath.Replace("{{AggregateRoot.Name}}", _settingsManager.Settings.Context.AggregateRoot.Name);
        outputPath = outputPath.Replace("{{AggregateRoot.NamePlural}}", _settingsManager.Settings.Context.AggregateRoot.NamePlural);

        // TODO: Replace in entity context
        //outputPath = outputPath.Replace("{{EntityName}}", _settingsManager.Settings.Context.Entity.Name);
        //outputPath = outputPath.Replace("{{EntityNamePlural}}", _settingsManager.Settings.Context.Entity.NamePlural);

        string templateText = File.ReadAllText(fullPath);
        SnippetTemplateContext templateContext = new SnippetTemplateContext();
        string templateSource = templateText;

        int templateConfigDelimiterIndex = templateText.IndexOf(_templateConfigDelimiter);
        if (templateConfigDelimiterIndex > -1)
        {
            templateContext = JsonConvert.DeserializeObject<SnippetTemplateContext>(templateText.Substring(0, templateConfigDelimiterIndex));
            templateSource = templateText.Substring(templateConfigDelimiterIndex + _templateConfigDelimiter.Length + Environment.NewLine.Length);
        }

        HandlebarsTemplate<object, object>? handlebarsTemplate = Handlebars.Compile(templateSource);
        string? templateOutput = handlebarsTemplate(_settingsManager.Settings.Context);

        if (template != null)
        {
            template.OutputPath = outputPath;
            template.Context = templateContext;
            template.Output = templateOutput;
        }
        else
        {
            template = new SnippetTemplate()
            {
                FullPath = fullPath,
                OutputPath = outputPath,
                Context = templateContext,
                Output = templateOutput
            };

            Templates.Add(template);
        }

        await _realtimeHub.Clients.All.SendAsync("SnippetTemplateLoaded", template);
    }

    public async Task LoadTemplatesAsync()
    {
        List<string> templateFilePaths = Directory.GetFiles(Path.GetDirectoryName(FileHelpers.UserBasedSnippetTemplatesPath) ?? "", "*.hbs", SearchOption.AllDirectories).ToList();

        foreach (string templateFilePath in templateFilePaths)
        {
            await LoadTemplateAsync(templateFilePath);
        }
    }
}