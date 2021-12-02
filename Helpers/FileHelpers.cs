using System.Reflection;

namespace SteffBeckers.Abp.Generator.Helpers;

public static class FileHelpers
{
    public static string GeneratorSettingsFileName => "generatorsettings.json";

    public static string GeneratorSettingsSchemaFileName => "generatorsettings.schema.json";

    public static string ProjectTemplatesPath => Path.Combine(TemplatesPath, "Projects");

    public static string SnippetTemplatesPath => Path.Combine(TemplatesPath, "Snippets");

    public static string TemplatesPath => Path.Combine(ContentRootPath, "Templates");

    public static string UserBasedGeneratorSettingsFilePath => Path.Combine(
        UserBasedPath,
        GeneratorSettingsFileName);

    public static string UserBasedPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".steffbeckers",
        "abp",
        "generator",
        Assembly.GetExecutingAssembly().GetName().Version.ToString(3));

    public static string UserBasedProjectTemplatesPath => Path.Combine(UserBasedTemplatesPath, "Projects");

    public static string UserBasedSnippetTemplatesPath => Path.Combine(UserBasedTemplatesPath, "Snippets");

    public static string UserBasedTemplatesPath => Path.Combine(UserBasedPath, "Templates");

#if DEBUG
    public static string ContentRootPath => Directory.GetCurrentDirectory();

    public static string WebRootPath => Path.Combine("wwwroot", "public");
#else
    public static string ContentRootPath { get => AppDomain.CurrentDomain.BaseDirectory; }

    public static string WebRootPath { get => Path.Combine("..", "..", "..", "staticwebassets", "public"); }
#endif

    public static void CopyFilesRecursively(string sourcePath, string targetPath)
    {
        // Create all of the directories
        foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
        }

        // Copy all the files and replaces any files with the same name
        foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
        {
            File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
        }
    }
}