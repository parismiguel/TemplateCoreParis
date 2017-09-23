using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IBM.WatsonDeveloperCloud.NaturalLanguageUnderstanding.v1;
using IBM.WatsonDeveloperCloud.NaturalLanguageUnderstanding.v1.Model;

namespace TemplateCoreParis.Controllers
{
    public class NluController : Controller
    {
        #region NLU parameters
        private static NaturalLanguageUnderstandingService _naturalLanguageUnderstandingService;

        static string userNLU = "41e3eac1-88e0-4ed9-aca2-b7c2d74bdcc1";
        static string pswNLU = "442L2dJkoqAO";

        string modelNLU = "";

        private string _nluText;
        private string _nluModel;
        #endregion


        public NluController()
        {
            _naturalLanguageUnderstandingService = new NaturalLanguageUnderstandingService(userNLU, pswNLU, NaturalLanguageUnderstandingService.NATURAL_LANGUAGE_UNDERSTANDING_VERSION_DATE_2017_02_27);
        }

        public IActionResult Index()
        {
            AnalysisResults model = new AnalysisResults();
            
            ViewData["modelID"] = "10:84885eae-87bf-42e8-9ddf-e41327d50429";

            try
            {
                model.AnalyzedText = System.IO.File.ReadAllText(@"..\NLUtest\wwwroot\Donde puedo comer el mejor lomo saltado.txt");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                model.AnalyzedText = "Luego de 10 años, volví a este restaurante. Sin duda alguna, su plato fuerte es la Sopa Seca con Carapulcra, es la mejor que he probado. La atención es A1. Eso sí, en fechas festivas es mejor ir desde temprano ya que el lugar, a pesar de ser grande, se llena rápidamente.";

                return View(model);
            }


            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(string AnalyzedText, string queryModelID)
        {
            _nluText = AnalyzedText;
            _nluModel = queryModelID;

            ViewData["modelID"] = _nluModel;

            AnalysisResults model = NaturalLanguageUnderstandingExample(userNLU, pswNLU);

            return View(model);
        }


        #region Constructor
        public AnalysisResults NaturalLanguageUnderstandingExample(string username, string password)
        {
            _naturalLanguageUnderstandingService = new NaturalLanguageUnderstandingService(username, password, NaturalLanguageUnderstandingService.NATURAL_LANGUAGE_UNDERSTANDING_VERSION_DATE_2017_02_27);

            AnalysisResults model = new AnalysisResults();

            model = Analyze(_nluModel, _nluText);
            return model;

        }
        #endregion

        #region Analyze
        public static AnalysisResults Analyze(string queryModelID, string _nluText)
        {
            _naturalLanguageUnderstandingService = new NaturalLanguageUnderstandingService(userNLU, pswNLU, NaturalLanguageUnderstandingService.NATURAL_LANGUAGE_UNDERSTANDING_VERSION_DATE_2017_02_27);

            List<string> model = new List<string>();

            List<string> _emotions = new List<string>()
            {
                "anger",
                "disgust",
                "fear",
                "joy",
                "sadness"
            };

            List<string> _targets = new List<string>()
            {
                "comida",
                "restaurante",
                "plato"
            };

            Parameters parameters = new Parameters()
            {
                Clean = true,
                FallbackToRaw = true,
                ReturnAnalyzedText = true,
                Features = new Features()
                {
                    Relations = new RelationsOptions(),
                    Sentiment = new SentimentOptions()
                    {
                        Document = true
                        //Targets = _targets
                    },
                    Emotion = new EmotionOptions()
                    {
                        Document = true
                        //Targets = _emotions
                    },
                    Keywords = new KeywordsOptions()
                    {
                        Limit = 50,
                        Sentiment = true,
                        Emotion = true
                    },
                    Entities = new EntitiesOptions()
                    {
                        Limit = 50,
                        Emotion = true,
                        Sentiment = true
                    },
                    Categories = new CategoriesOptions(),
                    Concepts = new ConceptsOptions()
                    {
                        Limit = 8
                    },
                    SemanticRoles = new SemanticRolesOptions()
                    {
                        Limit = 50,
                        Entities = true,
                        Keywords = true
                    }
                }
            };

            if (!string.IsNullOrEmpty(queryModelID))
            {
                parameters.Features.Relations.Model = queryModelID;
                parameters.Features.Entities.Model = queryModelID;
            }

            if (_nluText.StartsWith("http"))
            {
                parameters.Url = _nluText;
                parameters.Features.Metadata = new MetadataOptions();
            }
            else
            {
                parameters.Text = _nluText;
            }

            //parameters.Language = "en";

            Console.WriteLine(string.Format("\nAnalizando()..."));

            AnalysisResults result = _naturalLanguageUnderstandingService.Analyze(parameters);

            if (result != null)
            {
                if (!string.IsNullOrEmpty(result.Language))

                    model.Add(string.Format("Idioma: {0}", result.Language));

                if (!string.IsNullOrEmpty(result.AnalyzedText))

                    model.Add(string.Format("Texto analizado: {0}", result.AnalyzedText));

                if (!string.IsNullOrEmpty(result.RetrievedUrl))

                    model.Add(string.Format("URL recuperado: {0}", result.RetrievedUrl));

                if (result.Usage != null)
                {
                    if (result.Usage.Features != null)
                        model.Add(string.Format("Caracerísticas de uso: {0}", result.Usage.Features));
                }

                if (result.Concepts != null && result.Concepts.Count > 0)
                {
                    foreach (ConceptsResult conceptResult in result.Concepts)
                    {
                        model.Add(string.Format("Resultados de Concepto: {0}, Relevancia {1}, Fuente {2}", conceptResult.Text, conceptResult.Relevance, conceptResult.DbpediaResource));
                    }
                }

                if (result.Entities != null && result.Entities.Count > 0)
                {
                    foreach (EntitiesResult entityResult in result.Entities)
                    {
                        model.Add(string.Format("Tipo de Entidad: {0} | Relevancia: {1} | Total: {2} | Texto: {3}", entityResult.Type, entityResult.Relevance, entityResult.Count, entityResult.Text));

                        if (entityResult.Emotion != null)
                        {
                            EmotionScores emotionScores = entityResult.Emotion;
                            model.Add(string.Format("Enojo: {0} | Disgusto: {1} | Miedo: {2} | Alegría: {3} | Tristeza: {4}", emotionScores.Anger, emotionScores.Disgust, emotionScores.Fear, emotionScores.Joy, emotionScores.Sadness));
                        }

                        if (entityResult.Sentiment != null)
                        {
                            if (entityResult.Sentiment.Score != null)
                                model.Add("Puntaje Sentimiento: " + entityResult.Sentiment.Score);
                        }

                        if (entityResult.Disambiguation != null)
                        {
                            DisambiguationResult disambiguationResult = entityResult.Disambiguation;
                            model.Add(string.Format("Desambiguación de nombre: {0} | dbpediaResource: {1}", disambiguationResult.Name, disambiguationResult.DbpediaResource));

                            foreach (string type in disambiguationResult.Subtype)
                            {
                                model.Add("Sub tipo: " + type);
                            }
                        }
                    }
                }

                if (result.Keywords != null && result.Keywords.Count > 0)
                {
                    foreach (KeywordsResult keywordResult in result.Keywords)
                    {
                        model.Add(string.Format("Relevancia de palabras clave: {0}, Texto: {1}", keywordResult.Relevance, keywordResult.Text));

                        if (keywordResult.Emotion != null)
                        {
                            EmotionScores emotionScores = keywordResult.Emotion;
                            model.Add(string.Format("Enojo: {0} | Disgusto: {1} | Miedo: {2} | Alegría: {3} | Tristeza: {4}", emotionScores.Anger, emotionScores.Disgust, emotionScores.Fear, emotionScores.Joy, emotionScores.Sadness));
                        }

                        if (keywordResult.Sentiment != null)
                        {
                            model.Add("Puntaje Sentimiento: " + keywordResult.Sentiment.Score);
                        }
                    }
                }

                if (result.Categories != null && result.Categories.Count > 0)
                {
                    foreach (CategoriesResult categoryResult in result.Categories)
                    {
                        model.Add(string.Format("Categoría: {0} | Puntaje: {1}", categoryResult.Label, categoryResult.Score));
                    }
                }

                if (result.Emotion != null)
                {
                    if (result.Emotion.Document != null)
                    {
                        if (result.Emotion.Document.Emotion != null)
                        {
                            EmotionScores emotionScores = result.Emotion.Document.Emotion;
                            model.Add(string.Format("Enojo: {0} | Disgusto: {1} | Miedo: {2} | Alegría: {3} | Tristeza: {4}", emotionScores.Anger, emotionScores.Disgust, emotionScores.Fear, emotionScores.Joy, emotionScores.Sadness));
                        }
                    }

                    if (result.Emotion.Targets != null && result.Emotion.Targets.Count > 0)
                    {
                        foreach (TargetedEmotionResults targetedEmotionResult in result.Emotion.Targets)
                        {
                            model.Add(string.Format("Resultado Emoción: {0}", targetedEmotionResult.Text));

                            if (targetedEmotionResult.Emotion != null)
                            {
                                EmotionScores emotionScores = targetedEmotionResult.Emotion;
                                model.Add(string.Format("Enojo: {0} | Disgusto: {1} | Miedo: {2} | Alegría: {3} | Tristeza: {4}", emotionScores.Anger, emotionScores.Disgust, emotionScores.Fear, emotionScores.Joy, emotionScores.Sadness));
                            }
                        }

                    }
                }

                if (result.Metadata != null)
                {
                    MetadataResult metadata = result.Metadata;

                    if (metadata.Authors != null && metadata.Authors.Count > 0)
                    {
                        foreach (Author author in metadata.Authors)
                        {
                            model.Add("Autor: " + author.Name);
                        }
                    }
                }

                if (result.Relations != null && result.Relations.Count > 0)
                {
                    foreach (RelationsResult relationResult in result.Relations)
                    {
                        model.Add(string.Format("Puntaje Relación: {0} | Oración: {1} | Tipo: {2}", relationResult.Score, relationResult.Sentence, relationResult.Type));

                        if (relationResult.Arguments != null && relationResult.Arguments.Count > 0)
                        {
                            foreach (RelationArgument arg in relationResult.Arguments)
                            {
                                model.Add("Texto: " + arg.Text);

                                if (arg.Entities != null && arg.Entities.Count > 0)
                                {
                                    foreach (RelationEntity entity in arg.Entities)
                                    {
                                        model.Add(string.Format("Relación de Entidad: {0} | Tipo: {1}", entity.Text, entity.Type));
                                    }
                                }
                            }
                        }
                    }
                }

                if (result.SemanticRoles != null && result.SemanticRoles.Count > 0)
                {
                    foreach (SemanticRolesResult semanticRoleResult in result.SemanticRoles)
                    {
                        model.Add(string.Format("Rol semántica: {0}", semanticRoleResult.Sentence));

                        if (semanticRoleResult.Subject != null)
                        {
                            model.Add(string.Format("Sujeto semántica: {0}", semanticRoleResult.Subject.Text));

                            if (semanticRoleResult.Subject.Entities != null && semanticRoleResult.Subject.Entities.Count > 0)
                            {
                                foreach (SemanticRolesEntity entity in semanticRoleResult.Subject.Entities)
                                {
                                    model.Add(string.Format("Tipo Entidad: {0} | text: {1}", entity.Type, entity.Text));
                                }
                            }

                            if (semanticRoleResult.Subject.Keywords != null && semanticRoleResult.Subject.Keywords.Count > 0)
                            {
                                foreach (SemanticRolesKeyword keyword in semanticRoleResult.Subject.Keywords)
                                {
                                    model.Add(string.Format("Palabra clave: {0}", keyword.Text));
                                }
                            }
                        }

                        if (semanticRoleResult.Action != null)
                        {
                            model.Add(string.Format("Acción: {0} | Normalizado: {1}", semanticRoleResult.Action.Text, semanticRoleResult.Action.Normalized));

                            if (semanticRoleResult.Action.Verb != null)
                            {
                                model.Add(string.Format("Verbo: {0} | Frase: {1}", semanticRoleResult.Action.Verb.Text, semanticRoleResult.Action.Verb.Tense));
                            }
                        }

                        if (semanticRoleResult._Object != null)
                        {
                            model.Add(string.Format("Objeto: {0}", semanticRoleResult._Object.Text));

                            if (semanticRoleResult._Object.Keywords != null && semanticRoleResult._Object.Keywords.Count > 0)
                            {
                                foreach (SemanticRolesKeyword keyword in semanticRoleResult._Object.Keywords)
                                {
                                    model.Add("Palabra clave: " + keyword.Text);
                                }
                            }
                        }
                    }
                }

                if (result.Sentiment != null)
                {
                    if (result.Sentiment.Document != null)
                    {
                        model.Add("Puntaje Sentimiento del documento: " + result.Sentiment.Document.Score);

                        if (result.Sentiment.Targets != null && result.Sentiment.Targets.Count > 0)
                        {
                            foreach (TargetedSentimentResults targetedSentimentResult in result.Sentiment.Targets)
                            {
                                model.Add(string.Format("Resultados de Sentimiento objetivo: {0} | Puntaje: {1}", targetedSentimentResult.Text, targetedSentimentResult.Score));
                            }
                        }
                    }
                }
            }
            else
            {
                model.Add("Resultado es nulo");
            }


            return result;
        }
        #endregion



    }
}