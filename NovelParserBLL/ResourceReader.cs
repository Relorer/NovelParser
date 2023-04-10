using System.Reflection;

namespace NovelParserBLL;

public static class ResourceReader
{
    private const string resourcesDir = "Resources";
    private static readonly string basePath;
    private static readonly Assembly assembly;


    static ResourceReader()
    {
        assembly = Assembly.GetAssembly(typeof(ResourceReader)) 
                   ?? throw new ApplicationException("Can't get parser assembly");
        basePath = $"{assembly.GetName().Name}.{resourcesDir}";
    }

    public static string GetResourceData(string resourceName)
    {
        using var stream = GetResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static async Task<string> GetResourceDataAsync(string resourceName)
    {
        await using var stream = GetResourceStream(resourceName);
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public static byte[] GetResourceBinaryData(string resourceName)
    {
        using var stream = GetResourceStream(resourceName);
        using var reader = new BinaryReader(stream);
        return reader.ReadBytes((int)stream.Length);
    }

    public static async Task<byte[]> GetResourceBinaryDataAsync(string resourceName)
    {
        await using var stream = GetResourceStream(resourceName);
        using var reader = new BinaryReader(stream);
        return await Task.Run(() => reader.ReadBytes((int)stream.Length));
    }

    private static Stream GetResourceStream(string resourceName)
    {
        var resourcePath = $"{basePath}.{resourceName}";
        var stream = assembly.GetManifestResourceStream(resourcePath)
                     ?? throw new ApplicationException("Resource stream is not created.");
        return stream;
    }
}
