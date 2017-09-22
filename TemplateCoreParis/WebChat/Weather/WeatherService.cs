using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace IBM.VCA.WebChat.Weather
{

    public class WeatherService
    {
        static string city_id = "3936456";
        static string city_name = "Lima";
        static string country_code = "PE";
        static string weather_api = "ae9a7a50ee762e4e89f3841377f59bce";
        static string path = string.Format("forecast?id={0}&APPID={1}&units=metric&lang=es", city_id, weather_api);


    public static async Task<WeatherModel> GetWeatherAsync(string city, string date)
        {
            if (city != null)
            {
                city_name = city;
            }

            string path2 = string.Format("weather?q={0},{1}&APPID={2}&units=metric&lang=es", city_name, country_code, weather_api);

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync(path2);

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();


                    var weather = JsonConvert.DeserializeObject<WeatherModel>(result, new JsonSerializerSettings
                    {
                        Error = HandleDeserializationError
                    });

                    return weather;
                }
                else
                {
                    return null;
                }
            }

           

           
        }

        public static void HandleDeserializationError(object sender, ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }

    }

    
}