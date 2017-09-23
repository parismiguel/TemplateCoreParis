using IBM.WatsonDeveloperCloud.NaturalLanguageUnderstanding.v1.Model;
using Newtonsoft.Json;

namespace IBM.WatsonDeveloperCloud.Discovery.v1
{
    public class EnrichmentOptions2
    {
        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public Features Features { get; set; }
    }
}