using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
public static class JsonStorage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(),
        }
    };

    public static async Task SaveAsync<T>(string path, T value)
    {
        var json = JsonSerializer.Serialize(value, Options);
        await File.WriteAllTextAsync(path, json);
    }

    public static async Task<T?> LoadAsync<T>(string path)
    {
        if (!File.Exists(path))
            return default;
        
        var json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<T>(json, Options);
    }
}