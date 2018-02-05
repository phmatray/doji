// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using Doji;
//
//    var data = Exchange.FromJson(jsonString);

namespace Doji.Pages.UtilityPages.ExchangeUtility
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public partial class ExchangeJson
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("change")]
        public string Change { get; set; }

        [JsonProperty("logo")]
        public string Logo { get; set; }

        [JsonProperty("logo_url")]
        public string LogoUrl { get; set; }
    }

    public partial class ExchangeJson
    {
        public static List<ExchangeJson> FromJson(string json) => JsonConvert.DeserializeObject<List<ExchangeJson>>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<ExchangeJson> self) => JsonConvert.SerializeObject(self, Converter.Settings);
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
