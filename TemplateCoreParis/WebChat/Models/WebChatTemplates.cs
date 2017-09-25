using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateCoreParis.WebChat.Models
{
    public class WebChatTemplates
    {
        public class ButtonListTemplate
        {
            public List<ButtonTemplate> Buttons { get; set; }
        }

        public class CarouselTemplate
        {
            public static int _enum;

            public string CarouselName { get; set; }
            public List<ElementTemplate> Elements { get; set; }

            public CarouselTemplate()
            {
                _enum++;
            }
        }

        public class ButtonTemplate
        {
            public string Type { get; set; }
            public string HrefLink { get; set; }
            public string Text { get; set; }
        }

        public class ElementTemplate
        {
            public string Title { get; set; }
            public string Img_Url { get; set; }
            public List<ButtonTemplate> Buttons { get; set; }
        }


        public static string ButtonListConstructor(ButtonListTemplate buttons)
        {
            string model = string.Empty;

            foreach (var item in buttons.Buttons)
            {
                model = model + "<a class='btn btn-primary chatButton' href=" + item.HrefLink + ">" + item.Text + "</a>";
            }

            return model;
        }

        public static string CarouselConstructor(CarouselTemplate carousel)
        {
            string model = string.Empty;
            int totalElements = carousel.Elements.Count;

            model =
            "<div id='" + carousel.CarouselName + CarouselTemplate._enum + "' class='carousel slide chatCarousel' data-ride='carousel' data-interval='2000'>" +
           
            //"<ol class='carousel-indicators'>";

            //for (int i = 0; i < totalElements; i++)
            //{
            //    var _active = string.Empty;

            //    if (i == 0)
            //    {
            //        _active = "active";
            //    }

            //    model = model + 
            //        "<li data-target='#" + carousel.CarouselName + CarouselTemplate._enum + "' data-slide-to='" + i + "' class='" + _active + "'></li>";
            //}

            //model = model + "</ol>"

            "<div class='carousel-inner' role='listbox'>";

            int _index = 0;

            foreach (var item in carousel.Elements)
            {
                var _active = string.Empty;

                if (_index == 0)
                {
                    _active = " active";
                    _index = _index + 1;
                }

                model = model +
                    "<div class='item" + _active + "'>" +
                        "<img src='" + item.Img_Url + "' alt='" + item.Title + "' class='img-responsive'>" +
                        "<div class='carousel-caption' role='option'>" +
                        "<p>" + item.Title + "</p>";

                foreach (var _button in item.Buttons)
                {
                    model = model +
                         "<a class='btn btn-primary' target='_blank' href='" + _button.HrefLink + "'>" + _button.Text + "</a>";
                }

                model = model +
                        "</div>" +
                    "</div>";
            }

            model = model + 
            "</div>" +
            "<a class='left carousel-control' href='#" + carousel.CarouselName + CarouselTemplate._enum + "' role='button' data-slide='prev'>" +
                "<span class='glyphicon glyphicon-chevron-left' aria-hidden='true'></span>" +
                "<span class='sr-only'>Anterior</span>" +
            "</a>" +
            "<a class='right carousel-control' href='#" + carousel.CarouselName + CarouselTemplate._enum + "' role='button' data-slide='next'>" +
                "<span class='glyphicon glyphicon-chevron-right' aria-hidden='true'></span>" +
                "<span class='sr-only'>Siguiente</span>" +
            "</a>" +
            "</div>";

            return model;
        }


    }

}
