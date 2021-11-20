using HandlebarsDotNet;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;
using SteffBeckers.Abp.Generator.Settings;
using System.Diagnostics;

namespace SteffBeckers.Abp.Generator.Templates;

public class SnippetTemplatesAppService
{
    public readonly List<SnippetTemplate> Templates = new List<SnippetTemplate>();

    private readonly IHubContext<RealtimeHub> _realtimeHub;
    private readonly SettingsAppService _settingsAppService;
    private readonly string _templateConfigDelimiter = "#-#-#";

    public SnippetTemplatesAppService(
        IHubContext<RealtimeHub> realtimeHub,
        SettingsAppService settingsAppService)
    {
        _realtimeHub = realtimeHub;
        _settingsAppService = settingsAppService;
    }

    public Task<List<SnippetTemplate>> GetListAsync()
    {
        return Task.FromResult(Templates);
    }

    public async Task InitializeAsync()
    {
        if (!Directory.Exists(FileHelpers.UserBasedSnippetTemplatesPath))
        {
            Directory.CreateDirectory(FileHelpers.UserBasedSnippetTemplatesPath);
            FileHelpers.CopyFilesRecursively(FileHelpers.SnippetTemplatesPath, FileHelpers.UserBasedSnippetTemplatesPath);
        }

        await LoadTemplatesAsync();

        _settingsAppService.Monitor.OnChange(async (settings) => await LoadTemplatesAsync());

        FileSystemWatcher? watcher = new FileSystemWatcher(FileHelpers.UserBasedSnippetTemplatesPath, "*.hbs");
        watcher.IncludeSubdirectories = true;
        watcher.Changed += async (watch, eventArgs) =>
        {
            await LoadTemplateAsync(eventArgs.FullPath);
        };
        watcher.EnableRaisingEvents = true;
    }

    public async Task LoadTemplateAsync(string fullPath)
    {
        SnippetTemplate? template = Templates.FirstOrDefault(x => x.FullPath == fullPath);

        string outputPath = fullPath
            .Replace(FileHelpers.UserBasedSnippetTemplatesPath + Path.DirectorySeparatorChar, "")
            .Replace(".hbs", "");

        // TODO: Reflection based string replacement?
        outputPath = outputPath.Replace("{{Project.Name}}", _settingsAppService.Settings.Context.Project.Name);
        outputPath = outputPath.Replace("{{AggregateRoot.Name}}", _settingsAppService.Settings.Context.AggregateRoot.Name);
        outputPath = outputPath.Replace("{{AggregateRoot.NamePlural}}", _settingsAppService.Settings.Context.AggregateRoot.NamePlural);

        // TODO: Replace in entity context
        //outputPath = outputPath.Replace("{{EntityName}}", _settingsManager.Settings.Context.Entity.Name);
        //outputPath = outputPath.Replace("{{EntityNamePlural}}", _settingsManager.Settings.Context.Entity.NamePlural);

        string templateText;
        using (StreamReader? templateTextStreamReader = new StreamReader(File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
        {
            templateText = await templateTextStreamReader.ReadToEndAsync();
        }

        SnippetTemplateContext templateContext = new SnippetTemplateContext();
        string templateSource = templateText;

        int templateConfigDelimiterIndex = templateText.IndexOf(_templateConfigDelimiter);
        if (templateConfigDelimiterIndex > -1)
        {
            templateContext = JsonConvert.DeserializeObject<SnippetTemplateContext>(templateText.Substring(0, templateConfigDelimiterIndex));
            templateSource = templateText.Substring(templateConfigDelimiterIndex + _templateConfigDelimiter.Length + Environment.NewLine.Length);
        }

        HandlebarsTemplate<object, object>? handlebarsTemplate = Handlebars.Compile(templateSource);
        string? templateOutput = handlebarsTemplate(_settingsAppService.Settings.Context);

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

    public Task OpenFolderAsync()
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = FileHelpers.UserBasedSnippetTemplatesPath,
            UseShellExecute = true,
            Verb = "open"
        });

        return Task.CompletedTask;
    }
}