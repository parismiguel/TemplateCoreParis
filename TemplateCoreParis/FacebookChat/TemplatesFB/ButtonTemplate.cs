using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateCoreParis.FacebookChat.TemplatesFB.ButtonFB
{

    public class ButtonTemplate
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
        public string text { get; set; }
        public List<Button> buttons { get; set; }
    }

    public class Button
    {
        public string type { get; set; }
        public string url { get; set; }
        public string title { get; set; }
        public string payload { get; set; }
        public string webview_height_ratio { get; set; }
        public bool messenger_extensions { get; set; }
    }

}
