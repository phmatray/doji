namespace Doji.Pages.UtilityPages.GlossaryUtility
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public partial class GlossaryJson
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("categories")]
        public List<CategoryJson> Categories { get; set; }
    }

    public partial class CategoryJson
    {
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("elements")]
        public List<ElementJson> Elements { get; set; }
    }

    public partial class ElementJson
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public partial class GlossaryJson
    {
        public static GlossaryJson FromJson(string json) => JsonConvert.DeserializeObject<GlossaryJson>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this GlossaryJson self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    public class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
        };
    }
}
