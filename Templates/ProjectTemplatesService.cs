using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Settings;
using System.Diagnostics;
using System.Text;

namespace SteffBeckers.Abp.Generator.Templates;

    public class ProjectTemplatesService
    {
        private readonly SettingsService _settingsService;

        public List<ProjectTemplate> Templates = new List<ProjectTemplate>();

        public ProjectTemplatesService(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        private async Task LoadTemplatesAsync()
        {
            string? templateFilesDirectory = Path.GetDirectoryName(FileHelpers.UserBasedProjectTemplatesPath);

            if (string.IsNullOrEmpty(templateFilesDirectory))
            {
                throw new Exception($"Template files directory not found: {FileHelpers.UserBasedProjectTemplatesPath}");
            }

            List<string> templateSettingFilePaths = Directory
            .GetFiles(
                path: templateFilesDirectory,
                searchPattern: "templatesettings.json",
                searchOption: SearchOption.AllDirectories)
                .ToList();

            foreach (string templateSettingFilePath in templateSettingFilePaths)
            {
                string templateSettingFileText = await File.ReadAllTextAsync(templateSettingFilePath);

                ProjectTemplate? projectTemplate = JsonConvert.DeserializeObject<ProjectTemplate>(
                    templateSettingFileText);
                if (projectTemplate == null)
                {
                    continue;
                }

                projectTemplate.FullPath = Path.GetDirectoryName(templateSettingFilePath);

                Templates.Add(projectTemplate);
            }
        }

        private string ReplaceContextVariables(string text)
        {
            StringBuilder stringBuilder = new StringBuilder(text);

            stringBuilder.Replace("MyCompany.MyProduct", _settingsService.Settings.Context.Project.Name);
            stringBuilder.Replace("MyCompany", _settingsService.Settings.Context.Project.CompanyName);
            stringBuilder.Replace("MyProduct", _settingsService.Settings.Context.Project.ProductName);

            return stringBuilder.ToString();
        }

        public Task GenerateAsync(ProjectTemplateGenerateInputDto input)
        {
            ProjectTemplate? template = Templates.FirstOrDefault(x => x.Name == input.TemplateName);
            if (template == null || string.IsNullOrEmpty(template.FullPath))
            {
                return Task.CompletedTask;
            }

            string templateSourcePath = Path.Combine(template.FullPath, "Source");

            List<string> templateSourceFilePaths = Directory
            .GetFiles(path: templateSourcePath, searchPattern: "*", searchOption: SearchOption.AllDirectories)
                .ToList();

            return Parallel.ForEachAsync(
                templateSourceFilePaths,
                async (sourceFilePath, cancellationToken) =>
                {
                    string outputPath = sourceFilePath
                .Replace($"{templateSourcePath}{Path.DirectorySeparatorChar}", string.Empty)
                        .Replace(Path.DirectorySeparatorChar, '/');

                    string fullOutputPath = Path.Combine(_settingsService.Settings.ProjectPath, outputPath);
                    if (fullOutputPath == null)
                    {
                        return;
                    }

                    fullOutputPath = ReplaceContextVariables(fullOutputPath);

                    string? fullOutputDirectoryPath = Path.GetDirectoryName(fullOutputPath);
                    if (fullOutputDirectoryPath == null)
                    {
                        return;
                    }

                    if (!Directory.Exists(fullOutputPath))
                    {
                        Directory.CreateDirectory(fullOutputDirectoryPath);
                    }

                    string sourceFileText = await File.ReadAllTextAsync(sourceFilePath, cancellationToken);

                    sourceFileText = ReplaceContextVariables(sourceFileText);

                    await File.WriteAllTextAsync(fullOutputPath, sourceFileText, cancellationToken);
                });
        }

        public Task<List<ProjectTemplate>> GetListAsync()
        {
            return Task.FromResult(Templates);
        }

        public async Task InitializeAsync()
        {
            // Copy project templates from source to user based project templates folder.
            if (!Directory.Exists(FileHelpers.UserBasedProjectTemplatesPath))
            {
                Directory.CreateDirectory(FileHelpers.UserBasedProjectTemplatesPath);
                FileHelpers.CopyFilesRecursively(
                    FileHelpers.ProjectTemplatesPath,
                    FileHelpers.UserBasedProjectTemplatesPath);
            }

            // Load all project templates on startup.
            await LoadTemplatesAsync();

            // Reload all snippet templates when the settings change.
            _settingsService.Monitor.OnChange(async (settings) => await LoadTemplatesAsync());
        }

        public Task OpenFolderAsync()
        {
            Process.Start(
                new ProcessStartInfo()
                {
                    FileName = FileHelpers.UserBasedProjectTemplatesPath,
                    UseShellExecute = true,
                    Verb = "open"
                });

            return Task.CompletedTask;
        }
    }
