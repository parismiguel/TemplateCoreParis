using TemplateCoreParis.Helpers;
using TemplateCoreParis.Models;
using IBM.VCA.Watson.Watson.Model;
using IBM.VCA.WebChat.Weather;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static TemplateCoreParis.WebChat.GoogleUser;
using static TemplateCoreParis.WebChat.Models.WebChatTemplates;
using static IBM.VCA.Watson.Watson.WatsonConversationService;
using TemplateCoreParis.WebChat.Models;
using System.IO;
using System.Linq;
using TemplateCoreParis.Controllers;

namespace TemplateCoreParis.WebChat
{
    [Route("api/[controller]")]
    public class ChatBotController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private ISession _session => _httpContextAccessor.HttpContext.Session;

        static Context context;
        public static WatsonCredentials _credentials;

        public static string _keyEncode = "E546C8DF278CD5931069B522E695D4F2";

        private readonly UserManager<ApplicationUser> _userManager;

        public ChatBotController(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _credentials = _session.GetObjectFromJson<WatsonCredentials>("Watson");

            var _credsTest = HttpContext?.Session?.GetObjectFromJson<WatsonCredentials>("Watson");

            _userManager = userManager;
        }


        [HttpPost]
        public async Task<JsonResult> MessageChatAsync(string msg, bool isInit, bool isValid, string actionPayload)
        {
            try
            {
                string _attachment = null;

                if (_credentials == null)
                {
                    //IT-ORVAL=>Test-aje-jg->Aje group Paris
                    _credentials = new WatsonCredentials()
                    {
                        workspaceID = "ac6889fe-7f09-4f71-ac0e-4d8850b72d2f",
                        username = "7ecc7e1d-b7a9-472b-9419-a7254411cdd5",
                        password = "HQJwcbFZclYL"
                    };
                }

                if (isInit)
                {
                    context = new Context();

                }

                if (!string.IsNullOrEmpty(actionPayload))
                {
                    if (context == null)
                    {
                        context = new Context();
                    }

                    context.Action = actionPayload;
                }

                if (context != null)
                {
                    context.Validate = isValid;

                    switch (context.Action)
                    {
                        case "emailToValidate":

                            if (!context.Validate)
                            {
                                break;
                            }

                            var userEmailToValidate = await _userManager.FindByEmailAsync(msg);

                            if (userEmailToValidate?.EmailConfirmed == false || userEmailToValidate == null)
                            {
                                context.Validate = false;
                                break;
                            }

                            MyGoogleUserInfo _userinfo = GetGoogleUserInfo(msg);


                            if (_userinfo == null || _userinfo.Suspended == true)
                            {
                                context.Validate = false;
                                //context = null;
                            }
                            else
                            {
                                context.UserName = _userinfo.GivenName;
                                context.Email = msg;
                                context.Validate = true;
                            }

                            break;

                        case "secretToValidate":

                            if (string.IsNullOrEmpty(context.Email))
                            {
                                break;
                            }

                            var user = await _userManager.FindByEmailAsync(context.Email);
                            var _decrypt = Helpers.Helpers.DecryptString(user?.SecretResponse, _keyEncode);

                            //Default if not encrypted secret response previously
                            if (string.IsNullOrEmpty(_decrypt))
                            {
                                _decrypt = user?.SecretResponse;
                            }

                            if (msg == _decrypt)
                            {
                                context.Validate = true;
                                //context.Action = null;
                            }
                            else
                            {
                                context.Validate = false;
                                //context.Action = "emailToValidate";
                            }

                            break;

                        case "confirmationToValidate":
                            if (context.Validate == true && context.Password != null)
                            {
                                string goog = RunPasswordReset(context.Email, context.Password);

                                if (goog.Contains("Error") || goog.Contains("forbidden"))
                                {

                                    context.Action = null;
                                    context.Validate = false;
                                    context.Password = string.Empty;


                                    if (goog.Contains("forbidden"))
                                    {
                                        context.Error = "No tengo los privilegios suficientes para modificar la contraseña de un Administrador";
                                    }
                                    else
                                    {
                                        context.Error = "Ha sucedido un error al momento de intentar actualizar la contraseña";
                                    }

                                }
                            }

                            break;


                        default:
                            break;
                    }


                }


                MessageRequest result = Message(msg, context, _credentials);

                context.Error = null;

                if (result.Intents != null)
                {
                    string myIntent = result.Intents[0].IntentDescription;
                    string myAction = context?.Action;

                    switch (myIntent)
                    {
                        case "clima":
                            string _forecast = await CallWeatherAsync(null, null);

                            if (!string.IsNullOrEmpty(_forecast))
                            {
                                result.Output.Text.Add(_forecast);
                            }

                            break;

                        case "menu":
                            ButtonListTemplate _menu = new ButtonListTemplate()
                            {
                                Buttons = new List<ButtonTemplate>()
                            {
                                new ButtonTemplate() { HrefLink = "javascript:sendRequest(false,'ListPerfiles',true);", Text = "Listado de Perfiles" },
                                new ButtonTemplate() { HrefLink = "javascript:sendRequest(false,'AddServiceCall',true);", Text = "Crear Ticket" },
                            }
                            };

                            _attachment = ButtonListConstructor(_menu);

                            break;

                        case "productos":

                            _attachment = CarouselConstructor(GetCarouselList());

                            break;
                        default:
                            break;
                    }
                }


                //Testing purposes
                string codigo;

                switch (msg)
                {
                    case "productos":
                        _attachment = CarouselConstructor(GetCarouselList());
                        break;

                    case "producto":
                        codigo = "caros";

                        if (codigo != "undefined" && codigo != null)
                        {
                            var path = Path.Combine(HomeController._wwwRoot.WebRootPath, "data", "products.json");
                            string products;

                            using (StreamReader reader = System.IO.File.OpenText(path))
                            {
                                products = reader.ReadToEnd();
                            }

                            List<Producto> lista = JsonConvert.DeserializeObject<List<Producto>>(products);

                            Producto producto = lista.Where(x => x.Codigo == codigo).FirstOrDefault();

                            if (producto != null)
                            {
                            }
                                result.Output.Text.Add(string.Format("El precio de {0} es de {1} soles. Item consultado {2}",
                                    producto.Nombre, producto.Precio.ToString(), codigo));
                        }

                        break;

                    case "pedido":
                        codigo = "p-001001";

                        if (codigo != "undefined" && codigo != null)
                        {
                            var path = Path.Combine(HomeController._wwwRoot.WebRootPath, "data", "pedidos.json");
                            string pedidos;

                            using (StreamReader reader = System.IO.File.OpenText(path))
                            {
                                pedidos = reader.ReadToEnd();
                            }

                            List<Pedido> lista = JsonConvert.DeserializeObject<List<Pedido>>(pedidos);

                            Pedido pedido = lista.Where(x => x.NroPedido == codigo).FirstOrDefault();

                            if (pedido == null)
                            {
                                result.Output.Text.Add("No se encuentra el número de pedido indicado");
                            }
                            else
                            {
                                result.Output.Text.Add(string.Format("Su pedido con N° {0} para {1} se encuentra en ruta.",
                                    pedido.NroPedido, pedido.Nombre));
                            }

                        }

                        break;

                    default:
                        break;
                }


                context = result.Context;

                switch (context.Action)
                {
                    case "secretToValidate":

                        context.Validate = false;

                        if (string.IsNullOrEmpty(context.Email) || !IsValidEmail(context.Email))
                        {

                            break;
                        }

                        var user = await _userManager.FindByEmailAsync(context.Email);
                        var _decrypt = Helpers.Helpers.DecryptString(user?.SecretResponse, _keyEncode);

                        //Default if not encrypted secret response previously
                        if (string.IsNullOrEmpty(_decrypt))
                        {
                            _decrypt = user?.SecretResponse;
                        }

                        if (msg == _decrypt)
                        {
                            context.Validate = true;
                            break;
                        }

                        if (user != null)
                        {
                            result.Output.Text.Add(user.SecretQuestion);
                        }
                        else
                        {
                            result.Output = new OutputData()
                            {
                                Text = new List<string>()
                                {
                                    "Debe registrarse primero en la aplicación"
                                }
                            };

                            context = new Context();
                        }

                        break;

                    default:
                        break;
                }


                if (!string.IsNullOrEmpty(_attachment))
                {
                    result.Output.Text.Add(_attachment);
                }

                var json = JsonConvert.SerializeObject(result);

                return Json(json);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return null;
            }


        }


        private async Task<string> CallWeatherAsync(string city, string date)
        {
            var result = await WeatherService.GetWeatherAsync(city, date);
            string _forecast = null;

            if (result != null)
            {

                string _temperature = result.main.temp.ToString();
                string _city = result.name;
                string _description = result.weather[0].description;
                string _urlIcon = string.Format("../images/icons/{0}.png", result.weather[0].icon);


                _forecast =
                    "<div id='weather_widget' class='weather_widget'>" +
                    //"<div id= 'weather_widget_city_name' class='weather_widget_city_name'>Clima de " + city + "</div>" +
                    "<h3 id= 'weather_widget_temperature' class='weather_widget_temperature'>" +
                    "<img src='" + _urlIcon + "'> " + _temperature + "°C</h3>" +
                    "<div id='weather_widget_main' class='weather_widget_main'>" + _description + "</div>";


            }

            return _forecast;

        }

        private CarouselTemplate GetCarouselList()
        {
            CarouselTemplate _carousel = new CarouselTemplate()
            {
                CarouselName = "CarouselProductos",
                Elements = new List<ElementTemplate>()
                            {
                                new ElementTemplate()
                                {
                                    Img_Url = "images/products/ba15gira.jpg",
                                    Title = "Cool Tea",
                                    Buttons = new List<ButtonTemplate>()
                                    {
                                        new ButtonTemplate() { Text = "Ver más", HrefLink = "javascript:void();" }
                                    }
                                },
                                new ElementTemplate()
                                {
                                    Img_Url = "images/products/ca5tul.jpg",
                                    Title = "Volt",
                                    Buttons = new List<ButtonTemplate>()
                                    {
                                        new ButtonTemplate() { Text = "Ver más", HrefLink = "javascript:void();" }
                                    }
                                },
                                new ElementTemplate()
                                {
                                    Img_Url = "images/products/caros.jpg",
                                    Title = "Sporade",
                                    Buttons = new List<ButtonTemplate>()
                                    {
                                        new ButtonTemplate() { Text = "Ver más", HrefLink = "javascript:void();" }
                                    }
                                },
                                new ElementTemplate()
                                {
                                    Img_Url = "images/products/cu5miger.jpg",
                                    Title = "Cifrut",
                                    Buttons = new List<ButtonTemplate>()
                                    {
                                        new ButtonTemplate() { Text = "Ver más", HrefLink = "javascript:void();" }
                                    }
                                },
                                new ElementTemplate()
                                {
                                    Img_Url = "images/products/cu9ros.jpg",
                                    Title = "Big Cola",
                                    Buttons = new List<ButtonTemplate>()
                                    {
                                        new ButtonTemplate() { Text = "Ver más", HrefLink = "javascript:void();" }
                                    }
                                },
                                new ElementTemplate()
                                {
                                    Img_Url = "images/products/Flo4lilper.jpg",
                                    Title = "Cielo",
                                    Buttons = new List<ButtonTemplate>()
                                    {
                                        new ButtonTemplate() { Text = "Ver más", HrefLink = "javascript:void();" }
                                    }
                                }
                            }
            };

            return _carousel;

        }


        public bool IsValidEmail(string _email)
        {
            if (String.IsNullOrEmpty(_email))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                _email = Regex.Replace(_email, @"(@)(.+)$", this.DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }


            // Return true if strIn is in valid e-mail format.
            try
            {
                return Regex.IsMatch(_email,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                return "true";
            }
            return match.Groups[1].Value + domainName;
        }
    }


}
