using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;
using SteffBeckers.Abp.Generator.Settings;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SteffBeckers.Abp.Generator.Templates;

public class SnippetTemplatesService
{
    private readonly IHandlebars? _handlebarsContext = Handlebars.Create();
    private readonly IHubContext<RealtimeHub> _realtimeHub;
    private readonly SettingsService _settingsService;
    private readonly string _templateConfigDelimiter = "#-#-#";
    private List<SnippetTemplate> _templates = new List<SnippetTemplate>();

    public SnippetTemplatesService(IHubContext<RealtimeHub> realtimeHub, SettingsService settingsService)
    {
        _realtimeHub = realtimeHub;
        _settingsService = settingsService;

        HandlebarsHelpers.Register(_handlebarsContext);
    }

    public Task EditAsync(SnippetTemplateEditInputDto input)
    {
        Parallel.ForEach(
            input.OutputPaths,
            (string outputPath) =>
            {
                SnippetTemplate? snippetTemplate = _templates.FirstOrDefault(x => x.OutputPath == outputPath);

                if (snippetTemplate?.FullPath == null)
                {
                    return;
                }

                Process.Start(
                    new ProcessStartInfo()
                    {
                        FileName = snippetTemplate?.FullPath,
                        UseShellExecute = true,
                        Verb = "open"
                    });
            });

        return Task.CompletedTask;
    }

    public Task GenerateAsync(SnippetTemplateGenerateInputDto input)
    {
        return Parallel.ForEachAsync(
            input.OutputPaths,
            async (outputPath, cancellationToken) =>
            {
                SnippetTemplate? snippetTemplate = _templates.FirstOrDefault(x => x.OutputPath == outputPath);

                if (snippetTemplate?.OutputPath == null)
                {
                    return;
                }

                string fullOutputPath = Path.Combine(_settingsService.Settings.ProjectPath, snippetTemplate.OutputPath);

                if (fullOutputPath == null)
                {
                    return;
                }

                string? fullOutputDirectoryPath = Path.GetDirectoryName(fullOutputPath);

                if (fullOutputDirectoryPath == null)
                {
                    return;
                }

                if (!Directory.Exists(fullOutputPath))
                {
                    Directory.CreateDirectory(fullOutputDirectoryPath);
                }

                await File.WriteAllTextAsync(fullOutputPath, snippetTemplate.Output, cancellationToken);
            });
    }

    public Task<List<SnippetTemplate>> GetListAsync()
    {
        return Task.FromResult(_templates);
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
        _settingsService.Monitor
            .OnChange(
                async (settings) =>
                {
                    await LoadTemplatesAsync();

                    await _realtimeHub.Clients.All.SendAsync("SnippetTemplatesReloaded", _templates);
                });

        // Watch all template files for changes
        FileSystemWatcher? watcher = new FileSystemWatcher(FileHelpers.UserBasedSnippetTemplatesPath, "*.hbs")
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        watcher.Changed += async (watch, eventArgs) =>
        {
            await LoadTemplateAsync(eventArgs.FullPath);

            foreach (SnippetTemplate snippetTemplate in _templates.Where(x => x.FullPath == eventArgs.FullPath).ToList())
            {
                await _realtimeHub.Clients.All.SendAsync("SnippetTemplateUpdated", snippetTemplate);
            }
        };

        watcher.Renamed += async (watch, eventArgs) =>
        {
            _templates.RemoveAll(x => x.FullPath == eventArgs.OldFullPath);

            await _realtimeHub.Clients.All.SendAsync("SnippetTemplateDeleted", eventArgs.OldFullPath);
        };

        watcher.Deleted += async (watch, eventArgs) =>
        {
            _templates.RemoveAll(x => x.FullPath == eventArgs.FullPath);

            await _realtimeHub.Clients.All.SendAsync("SnippetTemplateDeleted", eventArgs.FullPath);
        };
    }

    public Task OpenFolderAsync()
    {
        Process.Start(
            new ProcessStartInfo()
            {
                FileName = FileHelpers.UserBasedSnippetTemplatesPath,
                UseShellExecute = true,
                Verb = "open"
            });

        return Task.CompletedTask;
    }

    private async Task LoadTemplateAsync(string fullPath)
    {
        try
        {
            string templateText;
            SnippetTemplateContext templateContext = new SnippetTemplateContext();
            string templateOutput = string.Empty;

            using (StreamReader? templateTextStreamReader =
                new StreamReader(File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                templateText = await templateTextStreamReader.ReadToEndAsync();
            }

            string templateSource = templateText;

            int templateConfigDelimiterIndex = templateText.IndexOf(_templateConfigDelimiter);

            if (templateConfigDelimiterIndex > -1)
            {
                templateContext = JsonConvert.DeserializeObject<SnippetTemplateContext>(
                    templateText.Substring(0, templateConfigDelimiterIndex)) ?? templateContext;

                templateSource = templateSource.Substring(
                    templateConfigDelimiterIndex + _templateConfigDelimiter.Length + Environment.NewLine.Length);
            }

            List<SnippetTemplate> generatedTemplates = new List<SnippetTemplate>();

            if (templateContext.RunForEachEntity)
            {
                foreach (Entity entity in _settingsService.Settings.Context.AggregateRoot.Entities)
                {
                    _settingsService.Settings.Context.Entity = entity;

                    generatedTemplates.Add(await ConvertToTemplateAsync(fullPath, templateContext, templateSource));
                }
            }
            else
            {
                generatedTemplates.Add(await ConvertToTemplateAsync(fullPath, templateContext, templateSource));
            }

            _templates.RemoveAll(x => x.FullPath == fullPath);
            _templates.AddRange(generatedTemplates);
            _templates = _templates.OrderBy(x => x.OutputPath).ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
    }

    private async Task<SnippetTemplate> ConvertToTemplateAsync(string fullPath, SnippetTemplateContext templateContext, string templateSource)
    {
        if (_handlebarsContext == null)
        {
            throw new Exception("Handlebars templating context could not be loaded.");
        }

        string outputPath = templateContext.OutputPath ?? fullPath
            .Replace($"{FileHelpers.UserBasedSnippetTemplatesPath}{Path.DirectorySeparatorChar}", string.Empty)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(".hbs", string.Empty);

        HandlebarsTemplate<object, object>? handlebarsOutputPath = _handlebarsContext.Compile(outputPath);
        outputPath = handlebarsOutputPath(_settingsService.Settings.Context);

        HandlebarsTemplate<object, object>? handlebarsTemplate = _handlebarsContext.Compile(templateSource);
        string output = handlebarsTemplate(_settingsService.Settings.Context);

        if (!string.IsNullOrEmpty(templateContext.Pattern) && !string.IsNullOrEmpty(templateContext.Replacement))
        {
            string fullOutputPath = Path.Combine(_settingsService.Settings.ProjectPath, outputPath);

            if (File.Exists(fullOutputPath))
            {
                string outputFileText = string.Empty;

                using (StreamReader? outputFileTextStreamReader =
                    new StreamReader(File.Open(fullOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    outputFileText = await outputFileTextStreamReader.ReadToEndAsync();
                }

                if (!outputFileText.Contains(output))
                {
                    output = Regex.Replace(
                        outputFileText,
                        templateContext.Pattern,
                        string.Format(templateContext.Replacement ?? string.Empty, output));
                }
                else
                {
                    output = outputFileText;
                }
            }
        }

        return new SnippetTemplate()
        {
            Context = templateContext,
            FullPath = fullPath,
            OutputPath = outputPath,
            Output = output
        };
    }

    private async Task LoadTemplatesAsync()
    {
        string? templateFilesDirectory = Path.GetDirectoryName(FileHelpers.UserBasedSnippetTemplatesPath);

        if (string.IsNullOrEmpty(templateFilesDirectory))
        {
            throw new Exception($"Template files directory not found: {FileHelpers.UserBasedSnippetTemplatesPath}");
        }

        List<string> templateFilePaths = Directory
        .GetFiles(path: templateFilesDirectory, searchPattern: "*.hbs", searchOption: SearchOption.AllDirectories)
            .ToList();

        _templates.Clear();

        foreach (string templateFilePath in templateFilePaths)
        {
            await LoadTemplateAsync(templateFilePath);
        }
    }
}