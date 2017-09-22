using TemplateCoreParis.FacebookChat;
using TemplateCoreParis.FacebookChat.TemplatesFB;
using TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB;
using TemplateCoreParis.FacebookChat.TemplatesFB.ListFB;
using TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB;
using TemplateCoreParis.FacebookChat.TemplatesFB.TextFB;

using TemplateCoreParis.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static IBM.WatsonDeveloperCloud.Conversation.v1.WatsonConversationService;
using IBM.WatsonDeveloperCloud.Conversation.v1.Model;
using TemplateCoreParis.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using TemplateCoreParis.Services;
using Tecactus.Api.Reniec;
using Twilio;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;

namespace TemplateCoreParis.FacebookChat
{
    [Route("api/[controller]")]
    public class FacebookBotController : Controller
    {

        static Dictionary<string, string> _phonesTwilio = new Dictionary<string, string>();

        static string access_token;

        static int maxDelayInMinutes;

        private WatsonCredentials credentials = new WatsonCredentials();

        private readonly ApplicationDbContext _context;

        private readonly IServiceProvider _provider;


        public FacebookBotController(ApplicationDbContext context, IServiceProvider provider)
        {
            var _enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            _context = context;

            _provider = provider;


            if (_enviroment == "Development")
            {
                //DEVELOPMENT: ChatBot VCA - PMP (Entel Peru Testing Bot)
                access_token = "EAADk45EJT9oBAEjqNBVTEj3ATmbWcH7v81Qn8uVnZCm9mIRxZBB97nf1ouy8ZA9f9Hy9H40DHxsAGrhtTs21psyZCPNZAZC389GKjk0vx26O2RTvwbQEMwdEN09I5hG8ZAnVegejhnyNopy6bYn3XZAzdSMPkQR6Ri5fQZAipiSZCKpAZDZD";
                maxDelayInMinutes = 2;

                credentials = new WatsonCredentials()
                {
                    //DEVELOPMENT: Organization=>ENTEL=>dev_entel_jg(ENTEL-PMP-08AGO17)
                    workspaceID = "a832a54a-38ee-43d2-906a-131a161c9043",
                    username = "8f62d0d6-6491-4c6f-9a11-ca151186e647",
                    password = "2T6hKrbXy01g"

                };

            }
            else
            {
                //PRODUCTION: VCAEntelDemoBot(VCA Chatbot Ecore)
                access_token = "EAAbWwHSM2tsBAOSimypQJEZBEyEZANZBIY4hUDD1bNV0MAxmhszqAzO3tbHcLN6AtBJToHk8kNvIiuVxrewqI5aKdKKgvjZAtruBUgZAalu0nLDJEjTeTgqp6mESJPuNIgpkWBLE27mSwW0ZCL5prZCra6moOjOXYevZBFCT7GEIPgZDZD";
                maxDelayInMinutes = 14;

                credentials = new WatsonCredentials()
                {

                    //PRODUCTION: Organization=>ENTEL=>dev_entel_jg (ENTEL-PMP-08AGO17(2))
                    workspaceID = "bf1c8e3b-9a3f-4f21-972b-cbb03da1f09c",
                    username = "8f62d0d6-6491-4c6f-9a11-ca151186e647",
                    password = "2T6hKrbXy01g"
                };

            }


        }

        [HttpGet]
        public ActionResult WebHook()
        {
            var query = HttpContext.Request.Query;


            if (query["hub.mode"].ToString() == "subscribe" &&
                query["hub.verify_token"].ToString() == "Esfuerzo1")
            {
                var retVal = query["hub.challenge"].ToString();

                if (retVal == null)
                {
                    return NotFound();
                }

                return Json(int.Parse(retVal));
            }
            else
            {
                return NotFound();
            }
        }


        [HttpPost]
        public ActionResult WebHook([FromBody] BotRequest data)
        {

            if (data == null || data?.entry?.Count == 0)
            {
                return new StatusCodeResult(StatusCodes.Status204NoContent);
            }

            try
            {

                var task = Task.Factory.StartNew(async () =>
                {
                    var senderId = data?.entry[0].messaging[0].sender?.id;
                    var recipientId = data?.entry[0].messaging[0].recipient?.id;

                    Context context;


                    using (IServiceScope scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {
                        ApplicationDbContext _contx = _provider.GetService<ApplicationDbContext>();

                        ContextWatsonFB contextWatsonFB = await _contx.ContextWatsonFB.Where(m => m.SenderId == senderId).FirstOrDefaultAsync();

                        if (contextWatsonFB == null)
                        {
                            context = null;
                        }
                        else
                        {
                            context = JsonConvert.DeserializeObject<Context>(contextWatsonFB.Context);
                        }

                    }


                    if (context == null || data?.entry[0].messaging[0].postback?.payload == "Rellamado" || data?.entry[0].messaging[0].postback?.payload == "rellamado")
                    {
                        context = new Context();
                        var _user = GetFbUserInfoAsync(senderId).Result;

                        context.User = new FbUserInfo()
                        {
                            first_name = _user.first_name,
                            last_name = _user.last_name,
                            profile_pic = _user.profile_pic
                        };
                    }

                    foreach (var entry in data.entry)
                    {
                        foreach (var message in entry.messaging)
                        {
                            var text = message?.message?.text;
                            var payload = message?.postback?.payload;

                            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(payload))
                            {
                                continue;
                            }


                            string msg = null;

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                msg = text;
                            }

                            if (!string.IsNullOrWhiteSpace(payload))
                            {
                                if (payload == "MenuPrincipal")
                                {
                                    msg = "hola";
                                }
                                else
                                {
                                    msg = payload;
                                }

                                if (payload == "human")
                                {
                                    context.Counter = 0;
                                }

                            }


                            if (!string.IsNullOrWhiteSpace(msg))
                            {
                                MessageRequest result = Message(msg, context, credentials);

                                context = result.Context;


                                //////////// Control Auto disconnection ///////////////////////////////////////////////
                                if (string.IsNullOrEmpty(context.LastPostTime))
                                {
                                    context.LastPostTime = DateTime.Now.ToUniversalTime().ToString();
                                }

                                DateTime start;
                                DateTime.TryParse(context.LastPostTime, out start);

                                var end = DateTime.Now.ToUniversalTime();

                                //DateTime.TryParseExact(context.LastPostTime, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out start);

                                int elapsedTime = (int)end.Subtract(start).TotalMinutes;


                                if (elapsedTime > maxDelayInMinutes)
                                {
                                    var sText = string.Format("Esperamos haber podido resolver tu consulta, gracias por escribirnos 😉", maxDelayInMinutes);

                                    var user = context.User;
                                    context = new Context();
                                    context.User = user;

                                    PostJson(GetMenuVolver(senderId, sText));
                                    break;
                                }

                                ////////////// Control max failed retry //////////////////////////////////////
                                if (result.Intents == null && !IsPhoneNumber(msg))
                                {
                                    context.Counter++;
                                }
                                else
                                {
                                    context.Counter = 0;
                                }

                                if (context.Counter > 1)
                                {
                                    PostJson(GetMenuHuman(senderId, "Permíteme un segundo, por favor, voy a conectarte con un 'Asesor Digital' que te ayudará con tu consulta."));
                                    context.Counter = 0;
                                    break;
                                }

                                ////////////// Post Watson Output Text ////////////////////////////////////////
                                foreach (var item in result.Output.Text)
                                {
                                    if (item == "")
                                    {
                                        continue;
                                    }
                                    //CallKeyWordFunc(item, message.sender.id, result);

                                    PostJson(GetTextFB(senderId, item));

                                    CallIntentsFunc(item, senderId, result);


                                };

                                if (!string.IsNullOrWhiteSpace(context.Action))
                                {
                                    CallContextActionAsync(context.Action, senderId, context);
                                }
                            }

                        }


                    }


                    using (IServiceScope scope = _provider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                    {

                        ApplicationDbContext _contx = _provider.GetService<ApplicationDbContext>();

                        ContextWatsonFB contextWatsonFB = await _contx.ContextWatsonFB.Where(m => m.SenderId == senderId).FirstOrDefaultAsync();

                        context.LastPostTime = DateTime.Now.ToUniversalTime().ToString();


                        if (contextWatsonFB == null)
                        {
                            contextWatsonFB = new ContextWatsonFB();
                            contextWatsonFB.RecipientId = recipientId;
                            contextWatsonFB.SenderId = senderId;

                            contextWatsonFB.Context = JsonConvert.SerializeObject(context);
                            contextWatsonFB.DateTimeUpdated = DateTime.Now;

                            _contx.Add(contextWatsonFB);
                        }
                        else
                        {
                            contextWatsonFB.Context = JsonConvert.SerializeObject(context);
                            contextWatsonFB.DateTimeUpdated = DateTime.Now;

                            _contx.Update(contextWatsonFB);

                        }

                        await _contx.SaveChangesAsync();
                    }

                });


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            return new StatusCodeResult(StatusCodes.Status200OK);
        }

        public async void CallContextActionAsync(string action, string senderId, Context context)
        {
            PostAction(SetActionFB(senderId, "typing_on"));

            switch (action)
            {

                case "MenuPrincipal":
                case "Principal":
                    PostJson(GetMenuPrincipal(senderId));
                    context.Counter = 0;

                    context.Action = null;
                    context.Mobile = null;
                    context.Validate = false;

                    break;

                case "Rellamado":
                    PostJson(GetMenuPrincipal(senderId));
                    context.Counter = 0;

                    context.Action = null;
                    context.Mobile = null;
                    context.Validate = false;

                    break;


                case "MenuLlamar":
                    PostJson(GetMenuLlamar(senderId));

                    context.Action = null;
                    context.Mobile = null;
                    context.Validate = false;

                    break;

                case "Decision":
                    PostJson(GetMenuDecision(senderId));
                    context.Action = null;

                    break;

                case "ReniecInfo":
                    TextTemplate _reniec = await GetReniecInfoAsync(context.DNI, senderId);

                    PostJson(_reniec);
                    PostJson(GetTextFB(senderId, "¿Cúal es el número que deseas migrar?"));

                    context.Action = null;
                    context.Validate = true;

                    break;

                case "SendSMSRequest":

                    if (IsPhoneNumber(context.Mobile))
                    {
                        var _number = string.Format("+51{0}", context.Mobile);
                        var _code = GenerateOTP();
                        context.Validate = true;

                        SendTwilio(_number, string.Format("Su código es: {0}", _code));

                        _phonesTwilio.Add(context.Mobile, _code);
                    }
                    else
                    {
                        PostJson(GetMenuVolver(senderId, "El número provisto no es válido"));
                        context.Mobile = null;
                        context.Action = null;
                        context.Validate = false;
                    }


                    break;

                case "ValidateSMS":
                    bool found = false;
                    string _tokenSent = string.Empty;

                    foreach (var item in _phonesTwilio)
                    {
                        if (item.Key == context.Mobile)
                        {
                            found = true;
                            _tokenSent = item.Value;

                            _phonesTwilio.Remove(item.Key);

                            break;
                        }
                    }

                    if (found)
                    {
                        if (context.Token == _tokenSent)
                        {
                            PostJson(GetTextFB(senderId, "Registro validado!"));
                            context.Login = true;
                        }
                        else
                        {
                            PostJson(GetTextFB(senderId, "Token incorrecto"));
                            context.Login = false;
                        }
                    }
                    else
                    {
                        PostJson(GetTextFB(senderId, "Token incorrecto"));
                        context.Mobile = null;
                    }

                    context.Action = null;

                    break;

                default:
                    break;
            }

            PostAction(SetActionFB(senderId, "typing_off"));

        }


        public void CallIntentsFunc(string item, string senderId, MessageRequest result)
        {
            if (result.Intents != null)
            {
                string myIntent = result.Intents[0].Intent;

                switch (myIntent)
                {


                    default:
                        break;
                }

            }

        }

        public static void SendTwilio(string mobile, string code)
        {
            try
            {
                // Find your Account Sid and Auth Token at twilio.com/console
                var accountSid = AuthMessageSender.Options.SMSAccountIdentification;
                var authToken = AuthMessageSender.Options.SMSAccountPassword;

                TwilioClient.Init(accountSid, authToken);

                var to = new PhoneNumber(mobile);
                var message = MessageResource.Create(
                    to,
                    from: new PhoneNumber(AuthMessageSender.Options.SMSAccountFrom),
                    body: code);

                Console.WriteLine(message.Sid);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.InnerException);
            }

        }

        protected string GenerateOTP()
        {
            string numbers = "1234567890";

            string characters = numbers;


            int length = 4;
            string otp = string.Empty;

            for (int i = 0; i < length; i++)
            {
                string character = string.Empty;
                do
                {
                    int index = new Random().Next(0, characters.Length);
                    character = characters.ToCharArray()[index].ToString();
                } while (otp.IndexOf(character) != -1);
                otp += character;
            }

            return otp;

        }

        public static async void PostJson(object data)
        {
            string url = "https://graph.facebook.com/v2.6/me/messages?access_token=" + access_token;

            var json = JsonConvert.SerializeObject(data);

            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";

                Stream dataStream = await request.GetRequestStreamAsync();

                using (var requestWriter = new StreamWriter(dataStream))
                {
                    requestWriter.Write(json);
                }

                WebResponse wr = await request.GetResponseAsync();

                Stream receiveStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
                string content = reader.ReadToEnd();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }


        }


        public static async void PostAction(object data)
        {
            string url = "https://graph.facebook.com/v2.6/me/messages?access_token=" + access_token;

            var json = JsonConvert.SerializeObject(data);

            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";

                Stream dataStream = await request.GetRequestStreamAsync();

                using (var requestWriter = new StreamWriter(dataStream))
                {
                    requestWriter.Write(json);
                }

                WebResponse wr = await request.GetResponseAsync();

                Stream receiveStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
                string content = reader.ReadToEnd();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }


        #region Functions FB API

        public static async Task<FbUserInfo> GetFbUserInfoAsync(string senderId)
        {
            string url = String.Format("https://graph.facebook.com/v2.6/{0}?fields=first_name,last_name,profile_pic&access_token={1}", senderId, access_token);

            FbUserInfo model = new FbUserInfo();

            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "application/json";

                WebResponse wr = await request.GetResponseAsync();

                Stream receiveStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
                string content = reader.ReadToEnd();

                model = JsonConvert.DeserializeObject<FbUserInfo>(content, new JsonSerializerSettings
                {
                    Error = HandleDeserializationError
                });

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return model;

        }

        public static TextTemplate GetTextFB(string senderId, string senderText)
        {

            TextTemplate model = new TextTemplate()
            {
                recipient = new TemplateCoreParis.FacebookChat.TemplatesFB.TextFB.Recipient()
                {
                    id = senderId
                },
                message = new TemplateCoreParis.FacebookChat.TemplatesFB.TextFB.Message()
                {
                    text = senderText
                }
            };

            return model;
        }

        public static ActionTemplate SetActionFB(string senderId, string actionText)
        {

            ActionTemplate model = new ActionTemplate()
            {
                recipient = new TemplateCoreParis.FacebookChat.TemplatesFB.TextFB.Recipient()
                {
                    id = senderId
                },
                sender_action = actionText
            };

            return model;
        }

        public static QuickRepliesTemplate GetMenuDecision(string senderId)
        {
            QuickRepliesTemplate model = new QuickRepliesTemplate()
            {
                recipient = new TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.Recipient()
                {
                    id = senderId
                },
                message = new TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.Message()
                {
                    text = "Elija por favor...",
                    quick_replies = new List<TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.QuickReply>()
                    {
                        new TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.QuickReply()
                        {
                            content_type = "text",
                            title = "Si",
                            payload = "ok"
                        },
                         new TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.QuickReply()
                        {
                            content_type = "text",
                            title = "No",
                            payload = "cancelar"
                        }
                    }
                }
            };

            return model;
        }

        public static QuickRepliesTemplate GetMenuVolver(string senderId, string _text)
        {
            QuickRepliesTemplate model = new QuickRepliesTemplate()
            {
                recipient = new TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.Recipient()
                {
                    id = senderId
                },
                message = new TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.Message()
                {
                    text = _text,
                    quick_replies = new List<TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.QuickReply>()
                    {
                        new TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.QuickReply()
                        {
                            content_type = "text",
                            title = "Volver",
                            payload = "rellamado"
                        }
                    }
                }
            };

            return model;
        }

        public static QuickRepliesTemplate GetMenuHuman(string senderId, string _text)
        {
            QuickRepliesTemplate model = new QuickRepliesTemplate()
            {
                recipient = new TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.Recipient()
                {
                    id = senderId
                },
                message = new TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.Message()
                {
                    text = _text,
                    quick_replies = new List<TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.QuickReply>()
                    {
                        new TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB.QuickReply()
                        {
                            content_type = "text",
                            title = "Si, por favor",
                            payload = "human"
                        }
                    }
                }
            };

            return model;
        }

        public static AttachmentTemplate GetMenuPrincipal(string senderId)
        {

            AttachmentTemplate model = new AttachmentTemplate()
            {
                recipient = new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Recipient()
                {
                    id = senderId
                },
                message = new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Message()
                {
                    attachment = new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Attachment()
                    {
                        type = "template",
                        payload = new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Payload()
                        {
                            template_type = "generic",
                            elements = new List<TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Element>()
                            {
                                new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Element()
                                {
                                    title = "Puedes obtener más información aquí:",
                                    buttons = new List<TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Button>()
                                    {
                                        new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Button()
                                        {
                                            type = "postback",
                                            title = "Portabilidad",
                                            payload = "portabilidad"
                                        },
                                        new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Button()
                                        {
                                            type = "postback",
                                            title = "Consulta de saldos",
                                            payload = "saldos"
                                        },
                                        new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Button()
                                        {
                                            type = "postback",
                                            title = "Estado de cuenta",
                                            payload = "estado"
                                        }
                                    }
                                }
                            }
                        }

                    }

                }
            };

            return model;
        }

        public AttachmentTemplate GetMenuLlamar(string senderId)
        {
            AttachmentTemplate model = new AttachmentTemplate()
            {
                recipient = new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Recipient()
                {
                    id = senderId
                },
                message = new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Message()
                {
                    attachment = new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Attachment()
                    {
                        type = "template",
                        payload = new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Payload()
                        {
                            template_type = "generic",
                            elements = new List<TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Element>()
                            {
                                new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Element()
                                {
                                    title = "Seleccione una opción",
                                    buttons = new List<TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Button>()
                                    {
                                        new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Button()
                                        {
                                            type = "phone_number",
                                            title = "Llámanos",
                                            payload = "+51994681214"
                                        },
                                        new TemplateCoreParis.FacebookChat.TemplatesFB.AttachmentFB.Button()
                                        {
                                            type = "postback",
                                            title = "Regresar 🏠",
                                            payload = "rellamado"
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            };

            return model;
        }


        #endregion



        private static async Task<TextTemplate> GetReniecInfoAsync(string _dni, string senderId)
        {
            TextTemplate model = new TextTemplate();

            try
            {
                var dni = new Dni("bki9O9ocOy5uRKCs00FNQzVoXv17CGXUFOKHQ69H");

                Person _person = await dni.getAsync(_dni);

                string _info = string.Format("Sus datos de la RENIEC son:\u000ANombres: {0}\u000AApellidos: {1} {2}", _person.nombres, _person.apellido_paterno, _person.apellido_materno);

                model = GetTextFB(senderId, _info);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Error");
                model = GetTextFB(senderId, ex.Message);

            }


            return model;
        }

        private static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();

            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool IsPhoneNumber(string number)
        {
            return Regex.Match(number, @"^([0-9]{9})$").Success;
        }

        public static void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            errorArgs.ErrorContext.Handled = true;
        }


    }

}