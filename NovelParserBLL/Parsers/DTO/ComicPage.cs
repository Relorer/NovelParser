using Newtonsoft.Json;

namespace NovelParserBLL.Parsers.DTO;

#nullable disable
public class ComicPage
{
    [JsonProperty("p")]
    public int Page { get; set; }
    [JsonProperty("u")]
    public string Url { get; set; }
}
#nullable restore