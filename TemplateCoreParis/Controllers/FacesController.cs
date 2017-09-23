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
        private VisualRecognitionService _visualRecognition = new VisualRecognitionService();

        private readonly IFaceServiceClient faceServiceClient;

        LanguageTranslatorService _languageTranslator = new LanguageTranslatorService();

        //FREE Edition - Visual Recognition IT-ORVAL->VCA
        private static string _apikey = "1035407f25f1e143bac6598920ddfae8786bec7e";

        ////PAID Edition - Visual Recognition-PMP IT-ORVAL->IoT
        //private static string _apikey = "563df392f73e5d7f55af50f37458216fb00ccbc2";


        public FacesController()
        {
            _visualRecognition.SetCredential(_apikey);
            _languageTranslator.SetCredential("eee5b78b-ab98-4b63-8e06-21b9595a5b05", "YVwhdDnnGGy5");

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



        [HttpPost]
        public IActionResult ClassifyGet(string _imageUrl)
        {
            var result = _visualRecognition.Classify(_imageUrl, null, null, 0, "es");

            List<string> model = new List<string>();

            if (result != null && result.Images != null)
            {
                foreach (ClassifyTopLevelSingle image in result.Images)
                    foreach (ClassifyPerClassifier classifier in image.Classifiers)
                        foreach (ClassResult classResult in classifier.Classes)
                            model.Add(string.Format("Clase: {0} | Puntaje: {1} | Tipo Jerarquía: {2}",
                                classResult._Class,
                                classResult.Score,
                                classResult.TypeHierarchy));


            }
            else
            {
                Console.WriteLine("No se encontraron resultados");
            }

            return Json(result);
        }

        [HttpPost]
        public IActionResult ClassifyGetCustom(string _imageUrl, string _classifier)
        {
            try
            {
                var test = _visualRecognition.GetClassifiersBrief();

                string[] classifiers = new string[0];

                if (!string.IsNullOrEmpty(_classifier))
                {
                    if (!classifiers.Contains(_classifier))
                    {
                        classifiers.Append(_classifier);
                    }

                }

                var result = _visualRecognition.Classify(_imageUrl, classifiers, null, 0, "es");

                return Json(result);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            //string[] classifiers = new string[] { "VCAStaff_627807617" };


            //if (!string.IsNullOrEmpty(_classifier))
            //{
            //    classifiers = new string[] { _classifier };
            //}

            //var result = _visualRecognition.Classify(_imageUrl, classifiers, null, 0, "es");

            //List<string> model = new List<string>();

            //if (result != null && result.Images != null)
            //{
            //    foreach (ClassifyTopLevelSingle image in result.Images)
            //        foreach (ClassifyPerClassifier classifier in image.Classifiers)
            //            foreach (ClassResult classResult in classifier.Classes)
            //                model.Add(string.Format("Clase: {0} | Puntaje: {1} | Tipo Jerarquía: {2}",
            //                    classResult._Class,
            //                    classResult.Score,
            //                    classResult.TypeHierarchy));


            //}
            //else
            //{
            //    Console.WriteLine("No se encontraron resultados");
            //}

            return Json(null);
        }


        [HttpPost]
        public IActionResult DetectFacesGet(string _faceUrl)
        {
            var result = _visualRecognition.DetectFaces(_faceUrl);

            if (result != null && result.Images != null)
            {
                if (result.Images != null && result.Images.Count > 0)
                {
                    foreach (FacesTopLevelSingle image in result.Images)
                    {
                        if (image.Faces != null && image.Faces.Count < 0)
                        {
                            foreach (OneFaceResult face in image.Faces)
                            {
                                if (face.Identity != null)
                                    Console.WriteLine(string.Format("name: {0} | score: {1} | type hierarchy: {2}", face.Identity.Name, face.Identity.Score, face.Identity.TypeHierarchy));
                                else
                                    Console.WriteLine("identity is null.");

                                if (face.Age != null)
                                    Console.WriteLine(string.Format("Age: {0} - {1} | score: {2}", face.Age.Min, face.Age.Max, face.Age.Score));
                                else
                                    Console.WriteLine("age is null.");

                                if (face.Gender != null)
                                    Console.WriteLine(string.Format("gender: {0} | score: {1}", face.Gender.Gender, face.Gender.Score));
                                else
                                    Console.WriteLine("gender is null.");
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No se encontraron resultados");
            }

            return Json(result);
        }



        [HttpPost]
        public IActionResult vCamAnalyze(string imgURL, string senderID)
        {
            TextTemplate response = new TextTemplate();
            string _text = string.Empty;

            try
            {

                var faces = _visualRecognition.DetectFaces(imgURL);

                if (faces != null && faces.Images[0].Faces.Count > 0)
                {
                    string _sex;
                    string _ageFrom = faces.Images[0].Faces[0].Age.Min.ToString();
                    string _ageTo = faces.Images[0].Faces[0].Age.Max.ToString();

                    switch (faces.Images[0].Faces[0].Gender.Gender)
                    {
                        case "FEMALE":
                            _sex = "una mujer";
                            break;

                        case "MALE":
                            _sex = "un hombre";
                            break;

                        default:
                            _sex = "una persona de sexo indeterminado";
                            break;
                    }

                    _text = string.Format("Veo {0} entre {1} a {2} años de edad. \u000A", _sex, _ageFrom, _ageTo);


                    //FacebookBotController.PostJson(response);
                }

                var classify = _visualRecognition.Classify(imgURL, null, null, 0, "es");

                if (classify != null && classify.Images != null)
                {
                    List<ClassResult> listClassResult = classify.Images[0].Classifiers[0].Classes;

                    ClassResult maxClass = listClassResult.Where(y => y.TypeHierarchy != null).OrderByDescending(x => x.Score).FirstOrDefault();

                    string classText = maxClass._Class;

                    TranslateResponse _translated = _languageTranslator.Translate("en", "es", classText);

                    _text = _text + string.Format("Su principal característica es {0}.", _translated.Translations[0].Translation);
                }

                //else
                //{
                //    var _errorText = "No se encontraron resultados";

                //    //FacebookBotController.PostJson(_errorText);

                //    return Json(_errorText);
                //}

                if (string.IsNullOrEmpty(_text))
                {
                    _text = "Se ha alcanzado el límite de uso por día (250 eventos)";
                }

                response = FacebookBotController.GetTextFB(senderID, _text);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
            }


            return Json(response);
        }


        [HttpPost]
        public IActionResult vCamAnalyzeBytes(byte[] imgBytes, string senderID, string mimeType)
        {
            TextTemplate response = new TextTemplate();
            string _textFaces = string.Empty;
            string _textCustom = string.Empty;
            string _textClassify = string.Empty;
            string fileName = string.Empty;
            string extension = string.Empty;
            string[] separator = { "/" };

            if (string.IsNullOrEmpty(mimeType))
            {
                mimeType = "image/png";
                fileName = "vCamTest.png";
            }
            else
            {
                extension = mimeType.Split(separator, StringSplitOptions.RemoveEmptyEntries)[1];
                fileName = string.Format("vCamTest.{0}", extension);
            }

            try
            {

                string[] classifiers = new string[0];

                var custom = _visualRecognition.Classify(imgBytes, fileName, mimeType, null, classifiers, null, 0, "es");


                if (custom != null && custom.Images != null)
                {
                    foreach (var item in custom.Images[0]._Classifiers)
                    {
                        ClassResult maxClass;

                        if (item.ClassifierId == "default")
                        {
                            maxClass = item.Classes.Where(y => y.TypeHierarchy != null).OrderByDescending(x => x.Score).FirstOrDefault();

                            string classText = maxClass._Class;

                            TranslateResponse _translated = _languageTranslator.Translate("en", "es", classText);

                            _textClassify = _textClassify + string.Format("Su principal característica es {0}. \u000A", _translated.Translations[0].Translation);
                        }
                        else
                        {
                            maxClass = item.Classes.OrderByDescending(x => x.Score).FirstOrDefault();

                            string classText = maxClass._Class;

                            if (maxClass.Score >= 0.55)
                            {
                                _textCustom = _textCustom + string.Format("Hola {0}! \u000A", classText);
                            }
                        }

                    }

                }

                var faces = _visualRecognition.DetectFaces(imgBytes, fileName, mimeType);

                if (faces != null && faces.Images[0].Faces.Count > 0)
                {
                    string _sex;
                    string _ageFrom = faces.Images[0].Faces[0].Age.Min.ToString();
                    string _ageTo = faces.Images[0].Faces[0].Age.Max.ToString();

                    switch (faces.Images[0].Faces[0].Gender.Gender)
                    {
                        case "FEMALE":
                            _sex = "una mujer";
                            break;

                        case "MALE":
                            _sex = "un hombre";
                            break;

                        default:
                            _sex = "una persona de sexo indeterminado";
                            break;
                    }

                    _textFaces = _textFaces + string.Format("Veo {0} entre {1} a {2} años de edad. \u000A", _sex, _ageFrom, _ageTo);


                }


                if (string.IsNullOrEmpty(_textClassify))
                {
                    _textClassify = "Se ha alcanzado el límite de uso por día (250 eventos)";
                }

                response = FacebookBotController.GetTextFB(senderID, _textCustom + _textFaces + _textClassify);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);
            }

            return Json(_textCustom + _textFaces + _textClassify);
        }


        [HttpPost]
        public IActionResult DetectFacesGetBytes(byte[] imgBytes, string fileName, string mimeType)
        {
            var result = _visualRecognition.DetectFaces(imgBytes);

            if (result != null)
            {
                if (result.Images != null && result.Images.Count > 0)
                {
                    foreach (FacesTopLevelSingle image in result.Images)
                    {
                        if (image.Faces != null && image.Faces.Count < 0)
                        {
                            foreach (OneFaceResult face in image.Faces)
                            {
                                if (face.Identity != null)
                                    Console.WriteLine(string.Format("name: {0} | score: {1} | type hierarchy: {2}", face.Identity.Name, face.Identity.Score, face.Identity.TypeHierarchy));
                                else
                                    Console.WriteLine("identity is null.");

                                if (face.Age != null)
                                    Console.WriteLine(string.Format("Age: {0} - {1} | score: {2}", face.Age.Min, face.Age.Max, face.Age.Score));
                                else
                                    Console.WriteLine("age is null.");

                                if (face.Gender != null)
                                    Console.WriteLine(string.Format("gender: {0} | score: {1}", face.Gender.Gender, face.Gender.Score));
                                else
                                    Console.WriteLine("gender is null.");
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No se encontraron resultados");
            }

            return Json(result);
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