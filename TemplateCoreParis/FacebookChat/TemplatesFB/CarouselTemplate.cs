using System.Collections.Generic;

namespace TemplateCoreParis.FacebookChat.TemplatesFB.CarouselFB
{
    public class CarouselTemplate
    {
        public Recipient recipient { get; set; }
        public Message message { get; set; }
    }

    public class Recipient
    {
        public string id { get; set; }
    }

    public class Message
    {
        public Attachment attachment { get; set; }
    }

    public class Attachment
    {
        public string type { get; set; }
        public Payload payload { get; set; }
    }

    public class Payload
    {
        public string template_type { get; set; }
        public List<Element> elements { get; set; }
    }

    public class Element
    {
        public string title { get; set; }
        public string subtitle { get; set; }
        public string image_url { get; set; }
        public List<Button> buttons { get; set; }

    }


    public class Button
    {
        public string payload { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

}