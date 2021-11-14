using HandlebarsDotNet;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;
using SteffBeckers.Abp.Generator.Settings;
using System.Diagnostics;

// TODO: From settings
GeneratorContext context = new GeneratorContext();

string contentRootPath = Directory.GetCurrentDirectory();
#if !DEBUG
contentRootPath = AppDomain.CurrentDomain.BaseDirectory;
#endif
string webRootPath = Path.Combine("wwwroot", "public");

string userBasedStoragePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".steffbeckers",
    "abp",
    "generator");
string templatesPath = Path.Combine(userBasedStoragePath, "Templates");
string snippetTemplatesPath = Path.Combine(templatesPath, "Snippets");

// Create user based directory if not exists and copy initial generator settings and templates
string generatorSettingsFileName = "generatorsettings.json";
if (!Directory.Exists(userBasedStoragePath))
{
    Directory.CreateDirectory(userBasedStoragePath);
    File.Copy(
        Path.Combine(contentRootPath, generatorSettingsFileName),
        Path.Combine(userBasedStoragePath, generatorSettingsFileName));

    Directory.CreateDirectory(templatesPath);
}

FileHelpers.CopyFilesRecursively(Path.Combine(contentRootPath, "Templates"), templatesPath);

// TODO: Extract to some template manager with a file watcher?
Dictionary<string, string> snippetTemplates = new Dictionary<string, string>();
List<string> snippetTemplateFilePaths = Directory.GetFiles(Path.GetDirectoryName(snippetTemplatesPath), "*.hbs", SearchOption.AllDirectories).ToList();
foreach (string snippetTemplateFilePath in snippetTemplateFilePaths)
{
    string outputPath = snippetTemplateFilePath
        .Replace(snippetTemplatesPath + Path.DirectorySeparatorChar, "")
        .Replace(".hbs", "");

    // TODO: Reflection based string replacement?
    outputPath = outputPath.Replace("{{ProjectName}}", context.ProjectName);
    outputPath = outputPath.Replace("{{AggregateRootName}}", context.AggregateRootName);
    outputPath = outputPath.Replace("{{AggregateRootNamePlural}}", context.AggregateRootNamePlural);

    if (!snippetTemplates.ContainsKey(outputPath))
    {
        string templateSource = File.ReadAllText(snippetTemplateFilePath);
        HandlebarsTemplate<object, object>? template = Handlebars.Compile(templateSource);
        string? output = template(context);

        snippetTemplates.Add(outputPath, output);
    }
}

// TODO: Remove
foreach (KeyValuePair<string, string> template in snippetTemplates)
{
    Console.WriteLine(template.Key);
    Console.WriteLine(template.Value);
    Console.WriteLine();
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(new WebApplicationOptions()
{
    Args = args,
    ContentRootPath = contentRootPath,
    WebRootPath = webRootPath
});

builder.Configuration.AddJsonFile(
    Path.Combine(userBasedStoragePath, generatorSettingsFileName),
    optional: true,
    reloadOnChange: true);

builder.Services
    .AddOptions<GeneratorSettings>()
    .Bind(builder.Configuration.GetRequiredSection("Generator"));
builder.Services.AddSingleton<GeneratorSettingsManager>();

builder.Services.AddSignalR();

WebApplication app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet(
    "/api/templates/open-folder",
    () =>
    {
        Process.Start(new ProcessStartInfo()
        {
            FileName = templatesPath,
            UseShellExecute = true,
            Verb = "open"
        });
    });

app.MapGet(
    "/api/templates/snippets",
    () =>
    {
        return snippetTemplates.Keys.ToList();
    });

app.MapGet(
    "/api/templates/snippets/{index}",
    (int index) =>
    {
        return snippetTemplates.Values.ElementAtOrDefault(index);
    });

app.MapGet(
    "/api/templates/snippets/{index}/generate",
    async (int index, GeneratorSettingsManager settingsManager) =>
    {
        string? snippetTemplatePath = snippetTemplates.Keys.ElementAtOrDefault(index);
        if (!string.IsNullOrEmpty(snippetTemplatePath))
        {
            string snippetTemplate = snippetTemplates.Values.ElementAt(index);

            string outputPath = Path.Combine(settingsManager.CurrentValue.ProjectPath, snippetTemplatePath);

            if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            }

            await File.WriteAllTextAsync(outputPath, snippetTemplate);
        }
    });

app.MapGet(
    "/api/settings",
    (GeneratorSettingsManager settingsManager) =>
    {
        return settingsManager.CurrentValue;
    });

app.MapPut(
    "/api/settings",
    async (GeneratorSettings input) =>
    {
        string json = JsonConvert.SerializeObject(
            new { Generator = input },
            Formatting.Indented);

        await File.WriteAllTextAsync(Path.Combine(userBasedStoragePath, generatorSettingsFileName), json);
    });

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<RealtimeHub>("/signalr-hubs/realtime");
});

Task run = app.RunAsync();

// Open browser on startup
string? url = app.Urls.FirstOrDefault();
if (url != null)
{
    BrowserHelper.OpenUrl(url);
}

await run;