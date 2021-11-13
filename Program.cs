﻿using HandlebarsDotNet;
using Newtonsoft.Json;
using SteffBeckers.Abp.Generator.Helpers;
using SteffBeckers.Abp.Generator.Realtime;
using SteffBeckers.Abp.Generator.Settings;
using System.Diagnostics;

string userBasedStoragePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".steffbeckers",
    "abp",
    "generator");
string generatorSettingsFileName = "generatorsettings.json";
string templatesPath = Path.Combine(userBasedStoragePath, "Templates");
string snippetTemplatesPath = Path.Combine(templatesPath, "Snippets");
string webRootPath = Path.Combine("wwwroot", "public");
GeneratorContext context = new GeneratorContext();

// Create user based directory if not exists and copy initial generator settings and templates
if (!Directory.Exists(userBasedStoragePath))
{
    Directory.CreateDirectory(userBasedStoragePath);
    File.Copy(
        Path.Combine(Directory.GetCurrentDirectory(), generatorSettingsFileName),
        Path.Combine(userBasedStoragePath, generatorSettingsFileName));

    Directory.CreateDirectory(templatesPath);
}

FileHelpers.CopyFilesRecursively("Templates", templatesPath);

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
#if !DEBUG
    ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
#endif
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
        return snippetTemplates.Keys.Select(x => x.ToString()).ToList();
    });

app.MapGet(
    "/api/templates/snippets/{index}",
    (int index) =>
    {
        return snippetTemplates.Values.ElementAt(index);
    });

app.MapGet(
    "/api/templates/snippets/{index}/generate",
    (int index) =>
    {
        // TODO: Generate template
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