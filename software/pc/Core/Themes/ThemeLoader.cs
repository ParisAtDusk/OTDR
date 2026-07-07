using System.IO;
using System.Text.Json;

public static class ThemeLoader
{
    public static ITheme LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<JsonTheme>(json)!;
    }
}