using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IBM.WatsonDeveloperCloud.Discovery.v1;
using System.Threading;
using IBM.WatsonDeveloperCloud.Discovery.v1.Model;
using Newtonsoft.Json;
using IBM.WatsonDeveloperCloud.NaturalLanguageUnderstanding.v1.Model;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Http;
using IBM.WatsonDeveloperCloud.NaturalLanguageUnderstanding.v1;

namespace TemplateCoreParis.Controllers
{
    public class DiscoveryController : Controller
    {
        #region Discovery Parameters
        public string _username = "20312444-1393-49f5-9bce-ab73bfc08c19";
        public string _password = "Z1DxzUeHnTP0";
        public string _endpoint;
        public DiscoveryService _discovery;

        private static string _createdEnvironmentId = "b1612bd9-6f9f-485f-84f4-c9d4740f509b";
        private static string _createdDocumentId;

        private string _filepathToIngest = @"DiscoveryTestData\watson_beats_jeopardy.html";
        private string _metadata = "{\"Creator\": \"Paris Pantigoso\",\"Subject\": \"Discovery example\"}";

        AutoResetEvent autoEvent = new AutoResetEvent(false);
        #endregion

        public DiscoveryController()
        {
            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);
        }

        public IActionResult Index()
        {
            //_discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            ViewData["Documents"] = GetDocuments();
            ViewData["queryDefault"] = "¿Donde puedo comer el mejor lomo saltado?";

            ViewData["modelID"] = "84a2997a-4fe8-410c-9de2-5103f41006c7";

            return View();
        }

        [HttpPost]
        public IActionResult QueryNLUparser(string queryRaw)
        {
            string _nluModel = "10:84885eae-87bf-42e8-9ddf-e41327d50429";

            AnalysisResults model = new AnalysisResults();

            model = NluController.Analyze(_nluModel, queryRaw);

            return Json(model);
        }

        #region Environments

        [HttpPost]
        public IActionResult GetEnvironments()
        {
            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            var result = _discovery.ListEnvironments();

            if (result != null)
            {
                //var model = JsonConvert.SerializeObject(result, Formatting.Indented);

                return Json(result);

            }
            else
            {
                return Json("Resultado nulo");
            }
        }


        #endregion

        #region Configurations

        [HttpPost]
        public IActionResult GetConfigurations()
        {
            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            var result = _discovery.ListConfigurations(_createdEnvironmentId);

            if (result != null)
            {
                //var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                return Json(result);
            }
            else
            {
                return Json("Resultado nulo");
            }
        }

        [HttpPost]
        public IActionResult GetConfiguration(string configid)
        {

            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            var result = _discovery.GetConfiguration(_createdEnvironmentId, configid);

            if (result != null)
            {
                //var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                return Json(result);
            }
            else
            {
                return Json("Resultado nulo");
            }

        }

        [HttpPost]
        public IActionResult CreateConfiguration(string configname, string modelid)
        {
            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);


            Conversions _conversions = new Conversions()
            {
                Pdf = new PdfSettings()
                {
                    Heading = new PdfHeadingDetection()
                    {
                        Fonts = new List<FontSetting>()
                        {
                           new FontSetting()
                           {
                               Level = (float)1.0,
                               MinSize = (float)24.0,
                               MaxSize = (float)80.0
                           },
                           new FontSetting()
                           {
                               Level = (float)2.0,
                               MinSize = (float)18.0,
                               MaxSize = (float)24.0,
                               Bold = false,
                               Italic = false
                           },
                           new FontSetting()
                           {
                               Level = (float)2.0,
                               MinSize = (float)18.0,
                               MaxSize = (float)24.0,
                               Bold = true
                           },
                           new FontSetting()
                           {
                               Level = (float)3.0,
                               MinSize = (float)13.0,
                               MaxSize = (float)18.0,
                               Bold = false,
                               Italic = false
                           },
                           new FontSetting()
                           {
                               Level = (float)3.0,
                               MinSize = (float)13.0,
                               MaxSize = (float)18.0,
                               Bold = true
                           },
                           new FontSetting()
                           {
                               Level = (float)4.0,
                               MinSize = (float)11.0,
                               MaxSize = (float)13.0,
                               Bold = false,
                               Italic = false
                           }
                        }
                    }
                },
                Word = new WordSettings()
                {
                    Heading = new WordHeadingDetection()
                    {
                        Fonts = new List<FontSetting>()
                        {
                           new FontSetting()
                           {
                               Level = (float)1.0,
                               MinSize = (float)24.0,
                               Bold = false,
                               Italic = false
                           },
                           new FontSetting()
                           {
                               Level = (float)2.0,
                               MinSize = (float)18.0,
                               MaxSize = (float)23.0,
                               Bold = true,
                               Italic = false
                           },
                            new FontSetting()
                           {
                               Level = (float)3.0,
                               MinSize = (float)14.0,
                               MaxSize = (float)17.0,
                               Bold = false,
                               Italic = false
                           },
                             new FontSetting()
                           {
                               Level = (float)4.0,
                               MinSize = (float)13.0,
                               MaxSize = (float)13.0,
                               Bold = true,
                               Italic = false
                           }
                        },
                        Styles = new List<WordStyle>()
                        {
                            new WordStyle()
                            {
                                Level = (float)1.0,
                                Names = new List<string>()
                                {
                                    "pullout heading","pulloutheading","header"
                                }
                            },
                            new WordStyle()
                            {
                                Level = (float)2.0,
                                Names = new List<string>()
                                {
                                    "subtitle"
                                }
                            }
                        }
                    }
                },
                Html = new HtmlSettings()
                {
                    ExcludeTagsCompletely = new List<string>()
                    {
                        "script","sup"
                    },
                    ExcludeTagsKeepContent = new List<string>()
                    {
                        "font","em","span"
                    },
                    KeepContent = new XPathPatterns()
                    {
                        Xpaths = new List<string>()
                    },
                    ExcludeContent = new XPathPatterns()
                    {
                        Xpaths = new List<string>()
                    },
                    ExcludeTagAttributes = new List<string>()
                    {
                        "EVENT_ACTIONS"
                    }
                },
                JsonNormalizations = new List<NormalizationOperation>()
            };

            Configuration2 configuration = new Configuration2()
            {
                Name = configname,
                Description = "Creado desde API",
                Conversions = _conversions,
                Enrichments = new List<Enrichment2>()
                {
                    new Enrichment2()
                    {
                        DestinationField = "enriched_text",
                        SourceField = "text",
                        EnrichmentName = "natural_language_understanding",
                        Options = new EnrichmentOptions2()
                    }
                }
            };



            Features _features = new Features()
            {
                Entities = new EntitiesOptions()
                {
                    Sentiment = true,
                    Emotion = true,
                    Limit = 20,
                    Model = modelid
                },
                Sentiment = new SentimentOptions()
                {
                    Document = true
                },
                Categories = new CategoriesOptions(),
                Concepts = new ConceptsOptions()
                {
                    Limit = 8
                },

                Relations = new RelationsOptions()
                {
                    Model = modelid
                },

                Emotion = new EmotionOptions()
                {
                    Document = true
                },
                Keywords = new KeywordsOptions()
                {
                    Limit = 20,
                    Sentiment = true,
                    Emotion = true
                },
                SemanticRoles = new SemanticRolesOptions()
                {
                    Limit = 8,
                    Entities = true,
                    Keywords = true
                }
            };


            if (!string.IsNullOrEmpty(modelid))
            {
                configuration.Enrichments[0].Options.Features = _features;
            }
            else
            {
                configuration.Enrichments[0].Options = new EnrichmentOptions2();
            }


            var result = _discovery.CreateConfiguration(_createdEnvironmentId, configuration);

            if (result != null)
            {
                var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                return Json(model);

            }
            else
            {
                return Json("Resultado nulo");
            }
        }

        [HttpPost]
        public IActionResult UpdateConfiguration(string configid, string modelid)
        {
            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            Conversions _conversions = new Conversions()
            {
                Pdf = new PdfSettings()
                {
                    Heading = new PdfHeadingDetection()
                    {
                        Fonts = new List<FontSetting>()
                        {
                           new FontSetting()
                           {
                               Level = (float)1.0,
                               MinSize = (float)24.0,
                               MaxSize = (float)80.0
                           },
                           new FontSetting()
                           {
                               Level = (float)2.0,
                               MinSize = (float)18.0,
                               MaxSize = (float)24.0,
                               Bold = false,
                               Italic = false
                           },
                           new FontSetting()
                           {
                               Level = (float)2.0,
                               MinSize = (float)18.0,
                               MaxSize = (float)24.0,
                               Bold = true
                           },
                           new FontSetting()
                           {
                               Level = (float)3.0,
                               MinSize = (float)13.0,
                               MaxSize = (float)18.0,
                               Bold = false,
                               Italic = false
                           },
                           new FontSetting()
                           {
                               Level = (float)3.0,
                               MinSize = (float)13.0,
                               MaxSize = (float)18.0,
                               Bold = true
                           },
                           new FontSetting()
                           {
                               Level = (float)4.0,
                               MinSize = (float)11.0,
                               MaxSize = (float)13.0,
                               Bold = false,
                               Italic = false
                           }
                        }
                    }
                },
                Word = new WordSettings()
                {
                    Heading = new WordHeadingDetection()
                    {
                        Fonts = new List<FontSetting>()
                        {
                           new FontSetting()
                           {
                               Level = (float)1.0,
                               MinSize = (float)24.0,
                               Bold = false,
                               Italic = false
                           },
                           new FontSetting()
                           {
                               Level = (float)2.0,
                               MinSize = (float)18.0,
                               MaxSize = (float)23.0,
                               Bold = true,
                               Italic = false
                           },
                            new FontSetting()
                           {
                               Level = (float)3.0,
                               MinSize = (float)14.0,
                               MaxSize = (float)17.0,
                               Bold = false,
                               Italic = false
                           },
                             new FontSetting()
                           {
                               Level = (float)4.0,
                               MinSize = (float)13.0,
                               MaxSize = (float)13.0,
                               Bold = true,
                               Italic = false
                           }
                        },
                        Styles = new List<WordStyle>()
                        {
                            new WordStyle()
                            {
                                Level = (float)1.0,
                                Names = new List<string>()
                                {
                                    "pullout heading","pulloutheading","header"
                                }
                            },
                            new WordStyle()
                            {
                                Level = (float)2.0,
                                Names = new List<string>()
                                {
                                    "subtitle"
                                }
                            }
                        }
                    }
                },
                Html = new HtmlSettings()
                {
                    ExcludeTagsCompletely = new List<string>()
                    {
                        "script","sup"
                    },
                    ExcludeTagsKeepContent = new List<string>()
                    {
                        "font","em","span"
                    },
                    KeepContent = new XPathPatterns()
                    {
                        Xpaths = new List<string>()
                    },
                    ExcludeContent = new XPathPatterns()
                    {
                        Xpaths = new List<string>()
                    },
                    ExcludeTagAttributes = new List<string>()
                    {
                        "EVENT_ACTIONS"
                    }
                },
                JsonNormalizations = new List<NormalizationOperation>()
            };

            Enrichment2 _enrichment = new Enrichment2()
            {
                DestinationField = "enriched_text",
                SourceField = "text",
                EnrichmentName = "natural_language_understanding",
                Options = new EnrichmentOptions2()
            };

            Features _features = new Features()
            {
                Entities = new EntitiesOptions()
                {
                    Sentiment = true,
                    Emotion = true,
                    Limit = 50,
                    Model = modelid
                },
                Sentiment = new SentimentOptions()
                {
                    Document = true
                },
                Categories = new CategoriesOptions(),
                Concepts = new ConceptsOptions()
                {
                    Limit = 10
                },

                Relations = new RelationsOptions()
                {
                    Model = modelid
                },

                Emotion = new EmotionOptions()
                {
                    Document = true
                },
                Keywords = new KeywordsOptions()
                {
                    Limit = 30,
                    Sentiment = true,
                    Emotion = true
                },
                SemanticRoles = new SemanticRolesOptions()
                {
                    Limit = 10,
                    Entities = true,
                    Keywords = true
                }
            };


            if (!string.IsNullOrEmpty(modelid))
            {
                _enrichment.Options.Features = _features;
            }
            else
            {
                _enrichment.Options = new EnrichmentOptions2();
            }


            Configuration2 config = _discovery.GetConfiguration(_createdEnvironmentId, configid);

            config.Enrichments[0] = _enrichment;
            config.Conversions = _conversions;


            var result = _discovery.UpdateConfiguration(_createdEnvironmentId, configid, config);

            if (result != null)
            {
                var model = JsonConvert.SerializeObject(result.Enrichments[0], Formatting.Indented);
                return Json(model);
            }
            else
            {
                return Json("Resultado nulo");
            }
        }

        [HttpPost]
        public IActionResult DeleteConfiguration(string configid)
        {
            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            var result = _discovery.DeleteConfiguration(_createdEnvironmentId, configid);

            if (result != null)
            {
                var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                return Json(model);
            }
            else
            {
                return Json("Resultado nulo");
            }
        }


        #endregion

        #region Collections

        [HttpPost]
        public IActionResult GetCollections()
        {
            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            var result = _discovery.ListCollections(_createdEnvironmentId);

            if (result != null)
            {
                //var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                return Json(result);
            }
            else
            {
                return Json("Resultado nulo");
            }
        }

        [HttpPost]
        public IActionResult GetCollection(string collectionid)
        {
            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            var result = _discovery.GetCollection(_createdEnvironmentId, collectionid);

            if (result != null)
            {
                //var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                return Json(result);
            }
            else
            {
                return Json("Resultado nulo");
            }
        }

        [HttpPost]
        public IActionResult CreateCollection(string name, string configid)
        {
            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            CreateCollectionRequest createCollectionRequest = new CreateCollectionRequest()
            {
                Language = CreateCollectionRequest.LanguageEnum.ES,
                Name = name,
                ConfigurationId = configid
            };

            var result = _discovery.CreateCollection(_createdEnvironmentId, createCollectionRequest);

            if (result != null)
            {
                var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                return Json(model);
            }
            else
            {
                return Json("Resultado nulo");
            }
        }

        [HttpPost]
        public IActionResult DeleteCollection(string collectionid)
        {

            if (string.IsNullOrEmpty(_createdEnvironmentId))
                throw new ArgumentNullException("_createdEnvironmentId es nulo");

            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            var result = _discovery.DeleteCollection(_createdEnvironmentId, collectionid);

            if (result != null)
            {
                var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                return Json(model);
            }
            else
            {
                return Json("Resultado nulo");
            }
        }


        #endregion

        #region Documents

        private string GetDocuments()
        {
            var collectionid = "9fe7fde3-3ffa-4773-856f-f88e2a8695e6";
            var query = "return=extracted_metadata&version=2017-08-01";
            //var query = "return=extracted_metadata";

            var result = _discovery.Query(_createdEnvironmentId, collectionid, null, query);

            if (result != null)
            {
                var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                return model;
            }
            else
            {
                return "Resultado nulo";
            }
        }

        [HttpPost]
        public IActionResult AddDocument(string collectionid, string configid)
        {
            //_createdEnvironmentId = "b1612bd9-6f9f-485f-84f4-c9d4740f509b"; //byod
            //collectionid = "9fe7fde3-3ffa-4773-856f-f88e2a8695e6"; //JamaPeruCollection
            //configid = "9726e2b6-5671-41c0-bd4d-2a9512ce02ee"; //Config20170801

            //_filepathToIngest = @"DiscoveryTestData\watson_beats_jeopardy.html";
            //_metadata = "{\"Creator\": \"Paris Pantigoso\",\"Subject\": \"Discovery example\"}";

            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);


            using (FileStream fs = System.IO.File.OpenRead(_filepathToIngest))
            {
                var result = _discovery.AddDocument(_createdEnvironmentId, collectionid, configid, fs as Stream, _metadata);

                if (result != null)
                {
                    var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                    _createdDocumentId = result.DocumentId;

                    return Json(result.DocumentId);
                }
                else
                {
                    return Json("Resultado nulo");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddDocumentList(List<string> list)
        {
            var collectionid = "9fe7fde3-3ffa-4773-856f-f88e2a8695e6"; //JamaPeruCollection
            var configid = "9726e2b6-5671-41c0-bd4d-2a9512ce02ee"; //Config20170801

            try
            {
                _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

                var listResult = new List<DocumentAccepted>();


                foreach (var item in list)
                {
                    using (HttpClient client = new HttpClient())
                    {

                        byte[] byteArray = await client.GetByteArrayAsync(item);
                        string _title;

                        using (HttpResponseMessage response = await client.GetAsync(item))
                        {
                            string fileType = response.Content.Headers.ContentType.MediaType;

                            using (HttpContent content = response.Content)
                            {
                                var source = await content.ReadAsStringAsync();
                                _title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                            }
                        }

                        //var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, item));

                        //string fileType = client.ResponseHeaders[HttpResponseHeader.ContentType];

                        //string source = client.DownloadString(item);


                        using (MemoryStream _stream = new MemoryStream(byteArray))
                        {
                            DocumentAccepted result = _discovery.AddDocument(_createdEnvironmentId, collectionid, configid, _stream as Stream, _metadata, null, _title);

                            if (result != null)
                            {
                                listResult.Add(result);
                            }
                        }

                    }
                }

                return Json(listResult);

            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }


        }



        [HttpPost]
        public IActionResult DetailsDocument(string collectionid, string documentid)
        {
            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            var result = _discovery.GetDocumentStatus(_createdEnvironmentId, collectionid, documentid);

            if (result != null)
            {
                //var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                return Json(result);
            }
            else
            {
                return Json("Resultado nulo");
            }
        }

        [HttpPost]
        public IActionResult DeleteDocument(string collectionid, string documentid)
        {
            _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

            var result = _discovery.DeleteDocument(_createdEnvironmentId, collectionid, documentid);

            if (result != null)
            {
                var model = JsonConvert.SerializeObject(result, Formatting.Indented);
                return Json(model);
            }
            else
            {
                return Json("Resultado nulo");
            }
        }

        #endregion


        #region Query

        [HttpPost]
        public IActionResult QueryRequest(string environmentid, string collectionid, string queryrequest)
        {
            QueryResponse model = new QueryResponse();
            string _text = string.Empty;

            try
            {
                _discovery = new DiscoveryService(_username, _password, DiscoveryService.DISCOVERY_VERSION_DATE_2017_08_01);

                var result = _discovery.Query(_createdEnvironmentId, collectionid, null, null, queryrequest);

                if (result != null)
                {
                    //model = JsonConvert.SerializeObject(result, Formatting.Indented);
                    model = result;

                    var maxDoc = result.Results.Where(y => y.enriched_text.relations.Count() > 0).OrderByDescending(x => x.Score).FirstOrDefault();

                    var maxDishOf = maxDoc.enriched_text.relations.Where(y => y.type == "dishOf").OrderByDescending(x => x.score).FirstOrDefault();

                    string foodDish, localName, qualityExpression;

                    foreach (var arg in maxDishOf.arguments)
                    {
                        foreach (var item in arg.entities)
                        {
                            switch (item.type)
                            {
                                case "FOOD_DISH":
                                    foodDish = item.text;
                                    break;
                                case "LOCAL_NAME":
                                    localName = item.text;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    var firstQualityOf = maxDoc.enriched_text.relations.Where(y => y.type == "qualityOf").FirstOrDefault();

                    var maxQualityOf = maxDoc.enriched_text.relations.Where(y => y.type == "qualityOf").OrderByDescending(x => x.score).FirstOrDefault();

                    foreach (var arg in maxQualityOf.arguments)
                    {
                        foreach (var item in arg.entities)
                        {
                            switch (item.type)
                            {
                                case "QUALITY_EXPRESSION":
                                    qualityExpression = item.text;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }

                    _text = string.Format("{0} ({1}%)", maxDishOf.sentence, Math.Round(maxDishOf.score * 100, 2));
                    _text += string.Format("<br/>{0} ({1}%)", firstQualityOf.sentence, Math.Round(firstQualityOf.score * 100, 2));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException);

                return Json(ex.Message);
            }

            return Json(new { obj = model, text = _text });
        }
        #endregion



    }
}