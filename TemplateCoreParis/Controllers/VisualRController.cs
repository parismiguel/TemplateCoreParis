using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IBM.WatsonDeveloperCloud.VisualRecognition.v3;
using TemplateCoreParis.FacebookChat;
using IBM.WatsonDeveloperCloud.LanguageTranslator.v2.Model;
using IBM.WatsonDeveloperCloud.VisualRecognition.v3.Model;
using TemplateCoreParis.FacebookChat.TemplatesFB.TextFB;
using IBM.WatsonDeveloperCloud.LanguageTranslator.v2;

namespace TemplateCoreParis.Controllers
{
    public class VisualRController : Controller
    {
        #region Visual Recognition parameters
        //FREE Edition - Visual Recognition IT-ORVAL->VCA
        private VisualRecognitionService _visualRecognition = new VisualRecognitionService();
        private static string _apikey = "1035407f25f1e143bac6598920ddfae8786bec7e";

        ////PAID Edition - Visual Recognition-PMP IT-ORVAL->IoT
        //private static string _apikey = "563df392f73e5d7f55af50f37458216fb00ccbc2";
        #endregion

        LanguageTranslatorService _languageTranslator = new LanguageTranslatorService();

        public VisualRController()
        {
            _languageTranslator.SetCredential("eee5b78b-ab98-4b63-8e06-21b9595a5b05", "YVwhdDnnGGy5");

            _visualRecognition.SetCredential(_apikey);
        }

        public IActionResult Index()
        {
            ViewData["Message"] = "Visual Recognition";

            ViewData["defaultURL"] = "https://pbs.twimg.com/profile_images/542897089957478400/_43mnSn9.png";

            // set the credentials
            _visualRecognition.SetCredential(_apikey);

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



    }
}