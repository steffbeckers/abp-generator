using HandlebarsDotNet;
using HandlebarsDotNet.Helpers;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;
using SteffBeckers.Abp.Generator.Settings;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
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

        // Loading in helpers from https://github.com/Handlebars-Net/Handlebars.Net.Helpers.
        HandlebarsHelpers.Register(_handlebarsContext);

        // String.Kebabcase helper.
        _handlebarsContext.RegisterHelper("String.Kebabcase", (output, context, arguments) =>
        {
            output.Write(
                Regex.Replace(
                    arguments[0].ToString() ?? string.Empty,
                    "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                    "-$1",
                    RegexOptions.Compiled)
                .Trim()
                .ToLower());
        });

        // String.SpacedPascalcase helper.
        _handlebarsContext.RegisterHelper("String.SpacedPascalcase", (output, context, arguments) =>
        {
            output.Write(
                Regex.Replace(
                    arguments[0].ToString() ?? string.Empty,
                    "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                    " $1",
                    RegexOptions.Compiled)
                .Trim());
        });

        // String.UppercasedSnakecase helper.
        _handlebarsContext.RegisterHelper("String.UppercasedSnakecase", (output, context, arguments) =>
        {
            output.Write(
                Regex.Replace(
                    arguments[0].ToString() ?? string.Empty,
                    "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
                    "_$1",
                    RegexOptions.Compiled)
                .Trim()
                .ToUpper());
        });

        // Override String.Camelcase helper.
        _handlebarsContext.RegisterHelper("String.Camelcase", (output, context, arguments) =>
        {
            output.Write(JsonNamingPolicy.CamelCase.ConvertName(arguments[0].ToString() ?? string.Empty));
        });
    }

    public Task<List<SnippetTemplateProjectFile>> GetProjectFileListAsync()
    {
        return Task.FromResult(Directory
            .GetFiles(
                path: _settingsService.Settings.ProjectPath,
                searchPattern: "*.*",
                searchOption: SearchOption.AllDirectories)
            .Where(x => x.EndsWith(".cs"))
            .Select(projectFilePath =>
            {
                string relativeProjectFilePath = projectFilePath
                    .Replace($"{_settingsService.Settings.ProjectPath}{Path.DirectorySeparatorChar}", string.Empty)
                    .Replace(Path.DirectorySeparatorChar, '/');

                return new SnippetTemplateProjectFile()
                {
                    RelativePath = relativeProjectFilePath
                };
            }).ToList());
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
        CopySnippetTemplates();

        // Load all snippet templates on startup.
        await LoadTemplatesAsync();

        // Reload all snippet templates when the settings change.
        _settingsService.Monitor.OnChange(async (settings) =>
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
        Process.Start(new ProcessStartInfo()
        {
            FileName = FileHelpers.UserBasedSnippetTemplatesPath,
            UseShellExecute = true,
            Verb = "open"
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Copy snippet templates from source to user based snippet templates folder.
    /// </summary>
    private static void CopySnippetTemplates()
    {
        if (!Directory.Exists(FileHelpers.UserBasedSnippetTemplatesPath))
        {
            Directory.CreateDirectory(FileHelpers.UserBasedSnippetTemplatesPath);
        }
        else
        {
            foreach (string filePath in Directory.GetFiles(
                path: FileHelpers.UserBasedSnippetTemplatesPath,
                searchPattern: "*",
                searchOption: SearchOption.AllDirectories))
            {
                // Don't delete custom snippets
                if (filePath.EndsWith("custom.hbs"))
                {
                    continue;
                }

                File.Delete(filePath);
            }
        }

        FileHelpers.CopyFilesRecursively(FileHelpers.SnippetTemplatesPath, FileHelpers.UserBasedSnippetTemplatesPath);
    }

    private async Task LoadTemplateAsync(string fullPath)
    {
        try
        {
            List<SnippetTemplate> generatedTemplates = new List<SnippetTemplate>();

            string templateText = string.Empty;
            string templateContextText = string.Empty;
            SnippetTemplateContext templateContext = new SnippetTemplateContext();
            string templateSource = string.Empty;

            string output = string.Empty;
            string outputPath = fullPath
                .Replace($"{FileHelpers.UserBasedSnippetTemplatesPath}{Path.DirectorySeparatorChar}", string.Empty)
                .Replace(Path.DirectorySeparatorChar, '/')
                .Replace(".hbs", string.Empty);

            if (_settingsService.Settings.Context.Project.IsModule)
            {
                if (outputPath.StartsWith("aspnet-core/"))
                {
                    outputPath = outputPath.Replace("aspnet-core/", $"aspnet-core/modules/{_settingsService.Settings.Context.Project.Name}/");
                }

                // TODO: What with angular frontend project?
            }

            string fullOutputPath = Path.Combine(_settingsService.Settings.ProjectPath, ReplaceVariables(outputPath));
            string outputFileText = string.Empty;
            if (!string.IsNullOrEmpty(fullOutputPath) && File.Exists(fullOutputPath))
            {
                using (StreamReader? outputFileTextStreamReader =
                    new StreamReader(File.Open(fullOutputPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    outputFileText = await outputFileTextStreamReader.ReadToEndAsync();
                }
            }

            using (StreamReader? templateTextStreamReader =
                new StreamReader(File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                templateText = await templateTextStreamReader.ReadToEndAsync();
            }

            string[] templateParts = templateText.Split(Environment.NewLine + _templateConfigDelimiter + Environment.NewLine);

            if (templateParts.Length == 1)
            {
                templateSource = templateParts[0];

                generatedTemplates.Add(new SnippetTemplate()
                {
                    Context = templateContext,
                    FullPath = fullPath,
                    Output = ReplaceVariables(templateSource),
                    OutputPath = ReplaceVariables(outputPath)
                });
            }
            else if (templateParts.Length > 1)
            {
                for (int i = 0; i < templateParts.Length; i = i + 2)
                {
                    templateContextText = templateParts[i];
                    templateContextText = ReplaceVariables(templateContextText);
                    templateContext = JsonConvert.DeserializeObject<SnippetTemplateContext>(templateContextText) ?? templateContext;

                    templateSource = templateParts[i + 1];

                    if (templateContext.RunForEachEntity)
                    {
                        if (_settingsService.Settings.Context.AggregateRoot.Entities.Count == 0)
                        {
                            return;
                        }

                        foreach (Entity entity in _settingsService.Settings.Context.AggregateRoot.Entities)
                        {
                            _settingsService.Settings.Context.Entity = entity;

                            generatedTemplates.Add(new SnippetTemplate()
                            {
                                Context = templateContext,
                                FullPath = fullPath,
                                Output = ReplaceVariables(templateSource),
                                OutputPath = ReplaceVariables(outputPath)
                            });
                        }
                    }
                    else if (!string.IsNullOrEmpty(outputFileText) &&
                        !string.IsNullOrEmpty(templateContext.RegexPattern) &&
                        !string.IsNullOrEmpty(templateContext.RegexReplacement))
                    {
                        _settingsService.Settings.Context.Entity = new Entity();

                        templateSource = ReplaceVariables(templateSource);

                        if (!outputFileText.Contains(templateSource))
                        {
                            outputFileText = Regex.Replace(
                                outputFileText,
                                templateContext.RegexPattern,
                                string.Format(templateContext.RegexReplacement ?? string.Empty, templateSource),
                                templateContext.RegexOptions);
                        }

                        output = outputFileText;
                    }
                }

                if (generatedTemplates.Count == 0)
                {
                    generatedTemplates.Add(new SnippetTemplate()
                    {
                        Context = templateContext,
                        FullPath = fullPath,
                        Output = output,
                        OutputPath = ReplaceVariables(outputPath)
                    });
                }
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

    private Task LoadTemplatesAsync()
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

        lock (_templates)
        {
            _templates.Clear();

            foreach (string templateFilePath in templateFilePaths)
            {
                LoadTemplateAsync(templateFilePath).GetAwaiter().GetResult();
            }
        }

        return Task.CompletedTask;
    }

    private string ReplaceVariables(string text)
    {
        if (_handlebarsContext == null)
        {
            return text;
        }

        HandlebarsTemplate<object, object>? handlebarsTemplate = _handlebarsContext.Compile(text);

        return handlebarsTemplate(_settingsService.Settings.Context);
    }
}