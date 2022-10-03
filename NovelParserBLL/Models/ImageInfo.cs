using Newtonsoft.Json;

namespace NovelParserBLL.Models
{
    public class ImageInfo
    {
        public ImageInfo(string directory, string url)
        {
            Name = $"{Guid.NewGuid()}.image";
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
        public string NameFromURL => URL.Substring(URL.LastIndexOf('/') + 1);

        public string FullPath { get; }
        public string Name { get; }
        public string URL { get; }

        public bool TryGetByteArray(out byte[]? img)
        {
            img = Exists ? File.ReadAllBytes(FullPath) : null;
            return img != null;
        }
    }
}