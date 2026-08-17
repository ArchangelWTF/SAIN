using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Utils;

namespace SAINServerMod.Utils;

[Injectable(InjectionType.Singleton)]
public sealed class JsonFileStoreUtil(FileUtil fileUtil)
{
    public async Task<T?> ReadAsync<T>(string path)
        where T : class
    {
        string? text = await ReadTextAsync(path);
        return string.IsNullOrWhiteSpace(text) ? null : SAINJsonUtil.Deserialize<T>(text);
    }

    public async Task WriteAsync(string path, object value)
    {
        await WriteTextAsync(path, SAINJsonUtil.Serialize(value));
    }

    public async Task<string?> ReadTextAsync(string path)
    {
        return fileUtil.FileExists(path) ? await fileUtil.ReadFileAsync(path) : null;
    }

    public async Task WriteTextAsync(string path, string text)
    {
        await fileUtil.WriteFileAsync(path, text);
    }

    public static string SanitizeFileName(string name)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Where(c => !invalid.Contains(c)));
    }
}
