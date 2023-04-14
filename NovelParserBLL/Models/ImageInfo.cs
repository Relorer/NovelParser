using Newtonsoft.Json;

namespace NovelParserBLL.Models;

public class ImageInfo
{
    public ImageInfo(string directory, string url)
    {
        var ext = Path.GetExtension(url);
        Name = $"{Guid.NewGuid()}{ext}";
        FullPath = Path.Combine(directory, Name);
        URL = url;
    }

    [JsonConstructor]
    public ImageInfo(string fullPath, string name, string url)
    {
        FullPath = fullPath;
        Name = name;
        URL = url;
    }

    [JsonIgnore]
    public bool Exists => !string.IsNullOrEmpty(FullPath) && File.Exists(FullPath);

    [JsonIgnore]
    public string NameFromURL => Path.GetFileName(URL);

    public string FullPath { get; }
    public string Name { get; }
    public string URL { get; }

    public bool TryGetByteArray(out byte[]? img)
    {
        img = Exists ? File.ReadAllBytes(FullPath) : null;
        return img != null;
    }

    public Task<byte[]?> GetByteArray()
    {
        return Task.Run(() => TryGetByteArray(out var bites) ? bites : null);
    }

    public override int GetHashCode() => HashCode.Combine(FullPath, Name, URL);
}