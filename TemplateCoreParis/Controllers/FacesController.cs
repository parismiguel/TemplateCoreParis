using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ProjectOxford.Face;
using Microsoft.AspNetCore.Mvc.Rendering;
using IBM.WatsonDeveloperCloud.VisualRecognition.v3;
using IBM.WatsonDeveloperCloud.LanguageTranslator.v2;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.ProjectOxford.Face.Contract;
using IBM.WatsonDeveloperCloud.VisualRecognition.v3.Model;
using TemplateCoreParis.FacebookChat;
using IBM.WatsonDeveloperCloud.LanguageTranslator.v2.Model;
using TemplateCoreParis.FacebookChat.TemplatesFB.TextFB;

namespace TemplateCoreParis.Controllers
{
    public class FacesController : Controller
    {
        private readonly IFaceServiceClient faceServiceClient;


        public FacesController()
        {

            faceServiceClient = new FaceServiceClient(MyKeys.subscriptionKey);
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            ViewData["defaultURL"] = "https://pbs.twimg.com/profile_images/542897089957478400/_43mnSn9.png";

            try
            {
                var list = await faceServiceClient.ListPersonGroupsAsync();
                ViewData["PersonGroups"] = new SelectList(list, "PersonGroupId", "Name");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return View();
        }

        public IActionResult VisualR()
        {

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> MSFaceDetect(string imageUrl)
        {

            try
            {

                Face[] faces = await faceServiceClient.DetectAsync(imageUrl, true, false,
                    new FaceAttributeType[] {
                        FaceAttributeType.Gender,
                        FaceAttributeType.Age,
                        FaceAttributeType.Smile,
                        FaceAttributeType.Glasses,
                        FaceAttributeType.FacialHair,
                        FaceAttributeType.HeadPose,
                        FaceAttributeType.Emotion,
                        FaceAttributeType.Hair,
                        FaceAttributeType.Makeup,
                        FaceAttributeType.Occlusion,
                        FaceAttributeType.Accessories,
                        FaceAttributeType.Blur,
                        FaceAttributeType.Exposure,
                        FaceAttributeType.Noise
                    });

                return Json(faces);

            }
            catch (FaceAPIException ex)
            {
                Console.WriteLine(ex.ErrorMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }


            return Json(null);
        }


        [HttpPost]
        public async Task<IActionResult> MSFaceDetectBytes(byte[] imgBytes, string senderID)
        {
            string _text = string.Empty;

            try
            {
                using (var stream = new MemoryStream(imgBytes))
                {
                    var faces = await faceServiceClient.DetectAsync(stream, true, false,
                        new FaceAttributeType[] {
                        FaceAttributeType.Gender,
                        FaceAttributeType.Age,
                        FaceAttributeType.Smile,
                        FaceAttributeType.Glasses,
                        FaceAttributeType.FacialHair,
                        FaceAttributeType.HeadPose,
                        FaceAttributeType.Emotion,
                        FaceAttributeType.Hair,
                        FaceAttributeType.Makeup,
                        FaceAttributeType.Occlusion,
                        FaceAttributeType.Accessories,
                        FaceAttributeType.Blur,
                        FaceAttributeType.Exposure,
                        FaceAttributeType.Noise
                        });


                    if (faces != null)
                    {
                        Dictionary<string, double> _emotionsList = new Dictionary<string, double>();
                        var data = faces[0].FaceAttributes.Emotion;

                        _emotionsList.Add("enojado", data.Anger);
                        _emotionsList.Add("con desprecio", data.Contempt);
                        _emotionsList.Add("indignado", data.Disgust);
                        _emotionsList.Add("con miedo", data.Fear);
                        _emotionsList.Add("feliz", data.Happiness);
                        _emotionsList.Add("indiferente", data.Neutral);
                        _emotionsList.Add("triste", data.Sadness);
                        _emotionsList.Add("sorprendido", data.Surprise);

                        var _emotionTop = _emotionsList.OrderByDescending(x => x.Value).FirstOrDefault().Key;

                        _text = string.Format("Te noto {0}", _emotionTop);
                    }
                    else
                    {
                        _text = "No se logra detectar un rostro";
                    }


                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }


            return Json(_text);

        }


        [HttpPost]
        public async Task<IActionResult> MsFaceIdentify(string imageUrl, string personGroupID)
        {

            try
            {
                if (string.IsNullOrEmpty(personGroupID))
                {
                    personGroupID = "35f912b2-fa73-40bf-aa47-5fcc5ba28d8a";
                }

                Face[] _faces = await faceServiceClient.DetectAsync(imageUrl, true, false,
                        new FaceAttributeType[] {
                        FaceAttributeType.Gender,
                        FaceAttributeType.Age,
                        FaceAttributeType.Smile,
                        FaceAttributeType.Glasses,
                        FaceAttributeType.FacialHair,
                        FaceAttributeType.HeadPose,
                        FaceAttributeType.Emotion,
                        FaceAttributeType.Hair,
                        FaceAttributeType.Makeup,
                        FaceAttributeType.Occlusion,
                        FaceAttributeType.Accessories,
                        FaceAttributeType.Blur,
                        FaceAttributeType.Exposure,
                        FaceAttributeType.Noise
               });

                Guid[] facesIds = _faces.Select(x => x.FaceId).ToArray();

                IdentifyResult[] _identify = await faceServiceClient.IdentifyAsync(personGroupID, facesIds);

                List<Person> _person = new List<Person>();

                foreach (var item in _identify)
                {
                    var value = item.Candidates;

                    if (value.Count() > 0)
                    {
                        var candidate = await faceServiceClient.GetPersonAsync(personGroupID, value[0].PersonId);
                        _person.Add(candidate);
                    }
                }

                return Json(new { identify = _identify, person = _person, faces = _faces });

            }
            catch (FaceAPIException ex)
            {
                Console.WriteLine(ex.ErrorMessage);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }


            return Json(null);
        }


        [HttpPost]
        public async Task<IActionResult> MsFaceIdentifyArray(byte[] imgBytes, string senderID)
        {
            string _text = string.Empty;

            try
            {
                var personGroupID = "35f912b2-fa73-40bf-aa47-5fcc5ba28d8a";

                using (var stream = new MemoryStream(imgBytes))
                {
                    Face[] _faces = await faceServiceClient.DetectAsync(stream, true, false,
                        new FaceAttributeType[] {
                            FaceAttributeType.Gender,
                            FaceAttributeType.Age,
                            FaceAttributeType.Smile,
                            FaceAttributeType.Glasses,
                            FaceAttributeType.FacialHair,
                            FaceAttributeType.HeadPose,
                            FaceAttributeType.Emotion,
                            FaceAttributeType.Hair,
                            FaceAttributeType.Makeup,
                            FaceAttributeType.Occlusion,
                            FaceAttributeType.Accessories,
                            FaceAttributeType.Blur,
                            FaceAttributeType.Exposure,
                            FaceAttributeType.Noise
                        });

                    if (_faces != null)
                    {
                        Dictionary<string, double> _emotionsList = new Dictionary<string, double>();
                        var data = _faces[0].FaceAttributes.Emotion;

                        _emotionsList.Add("enojado", data.Anger);
                        _emotionsList.Add("con desprecio", data.Contempt);
                        _emotionsList.Add("indignado", data.Disgust);
                        _emotionsList.Add("con miedo", data.Fear);
                        _emotionsList.Add("feliz", data.Happiness);
                        _emotionsList.Add("indiferente", data.Neutral);
                        _emotionsList.Add("triste", data.Sadness);
                        _emotionsList.Add("sorprendido", data.Surprise);

                        var _emotionTop = _emotionsList.OrderByDescending(x => x.Value).FirstOrDefault().Key;

                        _text = string.Format("Te noto {0}", _emotionTop);
                    }
                    else
                    {
                        _text = "No se logra detectar un rostro";
                    }


                    var facesIds = _faces.Select(x => x.FaceId).ToArray();

                    var _identify = await faceServiceClient.IdentifyAsync(personGroupID, facesIds);

                    Person _person = new Person();

                    if (_identify[0].Candidates.Count() > 0)
                    {
                        string personID = _identify[0].Candidates.OrderByDescending(x => x.Confidence).FirstOrDefault().PersonId.ToString();

                        _person = await faceServiceClient.GetPersonAsync(personGroupID, new Guid(personID));

                        _text = string.Format("Hola {0}. ", _person.Name) + _text;


                        return Json(_text);


                    }

                    return Json(_text);

                }


            }
            catch (FaceAPIException ex)
            {
                Console.WriteLine(ex.ErrorMessage);

                return Json(_text);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                return Json(_text);

            }

        }


        [HttpPost]
        public async Task<IActionResult> MsFaceIdentifyJson(byte[] imgBytes, string senderID)
        {
            string _text = string.Empty;

            if (imgBytes == null)
            {
                return Json(null);
            }

            try
            {
                var personGroupID = "35f912b2-fa73-40bf-aa47-5fcc5ba28d8a";

                using (var stream = new MemoryStream(imgBytes))
                {
                    Face[] _faces = await faceServiceClient.DetectAsync(stream, true, false,
                        new FaceAttributeType[] {
                            FaceAttributeType.Gender,
                            FaceAttributeType.Age,
                            FaceAttributeType.Smile,
                            FaceAttributeType.Glasses,
                            FaceAttributeType.FacialHair,
                            FaceAttributeType.HeadPose,
                            FaceAttributeType.Emotion,
                            FaceAttributeType.Hair,
                            FaceAttributeType.Makeup,
                            FaceAttributeType.Occlusion,
                            FaceAttributeType.Accessories,
                            FaceAttributeType.Blur,
                            FaceAttributeType.Exposure,
                            FaceAttributeType.Noise
                        });

                    if (_faces != null)
                    {
                        Dictionary<string, double> _emotionsList = new Dictionary<string, double>();
                        var data = _faces[0].FaceAttributes.Emotion;

                        _emotionsList.Add("enojado", data.Anger);
                        _emotionsList.Add("con desprecio", data.Contempt);
                        _emotionsList.Add("indignado", data.Disgust);
                        _emotionsList.Add("con miedo", data.Fear);
                        _emotionsList.Add("feliz", data.Happiness);
                        _emotionsList.Add("indiferente", data.Neutral);
                        _emotionsList.Add("triste", data.Sadness);
                        _emotionsList.Add("sorprendido", data.Surprise);

                        var _emotionTop = _emotionsList.OrderByDescending(x => x.Value).FirstOrDefault().Key;

                        _text = string.Format("Te noto {0}", _emotionTop);
                    }
                    else
                    {
                        _text = "No se logra detectar un rostro";
                    }


                    var facesIds = _faces.Select(x => x.FaceId).ToArray();

                    var _identify = await faceServiceClient.IdentifyAsync(personGroupID, facesIds);

                    Person _person = new Person();

                    if (_identify[0].Candidates.Count() > 0)
                    {
                        string personID = _identify[0].Candidates.OrderByDescending(x => x.Confidence).FirstOrDefault().PersonId.ToString();

                        _person = await faceServiceClient.GetPersonAsync(personGroupID, new Guid(personID));

                        _text = string.Format("Hola {0}. ", _person.Name) + _text;

                        return Json(new { text = _text, identify = _identify, person = _person, faces = _faces });


                    }

                    //return Json(_text);
                    return Json(new { text = _text, identify = _identify, faces = _faces });

                }


            }
            catch (FaceAPIException ex)
            {
                Console.WriteLine(ex.ErrorMessage);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);


            }

            return Json(null);
        }


        [HttpPost]
        public async Task<IActionResult> MakeAnalysisRequest(string imageFilePath)
        {
            HttpClient client = new HttpClient();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", MyKeys.subscriptionKey);

            // Request parameters. A third optional parameter is "details".
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            // Assemble the URI for the REST API Call.
            string uri = MyKeys.uriBase + "?" + requestParameters;

            HttpResponseMessage response = new HttpResponseMessage();

            try
            {
                // Request body. Posts a locally stored JPEG image.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses content type "application/octet-stream".
                    // The other content types you can use are "application/json" and "multipart/form-data".
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    // Execute the REST API call.
                    response = await client.PostAsync(uri, content);

                    // Get the JSON response.
                    string contentString = await response.Content.ReadAsStringAsync();

                    // Display the JSON response.
                    Console.WriteLine("\nResponse:\n");
                    Console.WriteLine(JsonPrettyPrint(contentString));

                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }


            return Json(response);

        }


        /// <summary>
        /// Returns the contents of the specified file as a byte array.
        /// </summary>
        /// <param name="imageFilePath">The image file to read.</param>
        /// <returns>The byte array of the image data.</returns>
        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }


        /// <summary>
        /// Formats the given JSON string by adding line breaks and indents.
        /// </summary>
        /// <param name="json">The raw JSON string to format.</param>
        /// <returns>The formatted JSON string.</returns>
        static string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }

    }
}