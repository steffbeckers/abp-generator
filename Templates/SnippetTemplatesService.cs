using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;
using SteffBeckers.Abp.Generator.Settings;
using System.Diagnostics;

namespace SteffBeckers.Abp.Generator.Templates;

public class SnippetTemplatesService
{
    public readonly List<SnippetTemplate> Templates = new List<SnippetTemplate>();

    private readonly IHandlebars? _handlebarsContext = Handlebars.Create();
    private readonly IHubContext<RealtimeHub> _realtimeHub;
    private readonly SettingsService _settingsAppService;
    private readonly string _templateConfigDelimiter = "#-#-#";

    public SnippetTemplatesService(
        IHubContext<RealtimeHub> realtimeHub,
        SettingsService settingsAppService)
    {
        _realtimeHub = realtimeHub;
        _settingsAppService = settingsAppService;

        HandlebarsHelpers.Register(_handlebarsContext);
    }

    public Task<List<SnippetTemplate>> GetListAsync()
    {
        return Task.FromResult(Templates);
    }

    public async Task InitializeAsync()
    {
        // Copy snippet templates from source to user based snippet templates folder.
        if (!Directory.Exists(FileHelpers.UserBasedSnippetTemplatesPath))
        {
            Directory.CreateDirectory(FileHelpers.UserBasedSnippetTemplatesPath);
            FileHelpers.CopyFilesRecursively(FileHelpers.SnippetTemplatesPath, FileHelpers.UserBasedSnippetTemplatesPath);
        }

        // Load all snippet templates on startup.
        await LoadTemplatesAsync();

        // Reload all snippet templates when the settings change.
        _settingsAppService.Monitor.OnChange(async (settings) =>
        {
            await LoadTemplatesAsync();
            await _realtimeHub.Clients.All.SendAsync("SnippetTemplatesReloaded", Templates);
        });

        // Watch all template files for changes
        FileSystemWatcher? watcher = new FileSystemWatcher(FileHelpers.UserBasedSnippetTemplatesPath, "*.hbs")
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        watcher.Created += async (watch, eventArgs) =>
        {
            SnippetTemplate snippetTemplate = await LoadTemplateAsync(eventArgs.FullPath);

            await _realtimeHub.Clients.All.SendAsync("SnippetTemplateCreated", snippetTemplate);
        };

        watcher.Changed += async (watch, eventArgs) =>
        {
            SnippetTemplate snippetTemplate = await LoadTemplateAsync(eventArgs.FullPath);

            await _realtimeHub.Clients.All.SendAsync("SnippetTemplateUpdated", snippetTemplate);
        };

        watcher.Deleted += async (watch, eventArgs) =>
        {
            SnippetTemplate? snippetTemplateToDelete = Templates.FirstOrDefault(x => x.FullPath == eventArgs.FullPath);

            if (snippetTemplateToDelete != null)
            {
                Templates.Remove(snippetTemplateToDelete);
            }

            await _realtimeHub.Clients.All.SendAsync("SnippetTemplateDeleted", eventArgs.FullPath);
        };
    }

    public async Task<SnippetTemplate> LoadTemplateAsync(string fullPath)
    {
        SnippetTemplate? template = Templates.FirstOrDefault(x => x.FullPath == fullPath);

        string outputPath = fullPath
            .Replace(FileHelpers.UserBasedSnippetTemplatesPath + Path.DirectorySeparatorChar, "")
            .Replace(".hbs", "");

        // TODO: Replace based on handlebars?
        // HandlebarsTemplate<object, object>? handlebarsOutputPath = _handlebarsContext.Compile(outputPath);
        // outputPath = handlebarsOutputPath(_settingsService.Settings.Context);
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
            templateContext = JsonConvert.DeserializeObject<SnippetTemplateContext>(templateText.Substring(0, templateConfigDelimiterIndex)) ?? templateContext;
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

        return template;
    }

    public async Task LoadTemplatesAsync()
    {
        string? templateFilesDirectory = Path.GetDirectoryName(FileHelpers.UserBasedSnippetTemplatesPath);

        if (string.IsNullOrEmpty(templateFilesDirectory))
        {
            throw new Exception($"Template files directory not found: {FileHelpers.UserBasedSnippetTemplatesPath}");
        }

        List<string> templateFilePaths = Directory
            .GetFiles(
                path: templateFilesDirectory,
                searchPattern: "*.hbs",
                searchOption: SearchOption.AllDirectories)
            .ToList();

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