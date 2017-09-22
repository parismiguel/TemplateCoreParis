using TemplateCoreParis.FacebookChat.TemplatesFB;
using Newtonsoft.Json;

namespace TemplateCoreParis.FacebookChat
{
   
    public class ContextCustom
    {
        [JsonProperty("temperature")]
        public string Temperature { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("servicio")]
        public string Servicio { get; set; }

        [JsonProperty("codigo")]
        public string Codigo { get; set; }

        [JsonProperty("mobile")]
        public string Mobile { get; set; }

        [JsonProperty("login")]
        public bool Login { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("dni")]
        public string DNI { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("validate")]
        public bool Validate { get; set; }

        [JsonProperty("locacion")]
        public string Locacion { get; set; }

        [JsonProperty("tipo_promo")]
        public string Tipo_Promo { get; set; }

        [JsonProperty("counter")]
        public int Counter { get; set; }

        [JsonProperty("lastposttime")]
        public string LastPostTime { get; set; }

        [JsonProperty("user")]
        public FbUserInfo User { get; set; }
    }
}