using System.Collections.Generic;

namespace TemplateCoreParis.FacebookChat.TemplatesFB.QuickRepliesFB
{
    public class QuickRepliesTemplate
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
        public string text { get; set; }
        public List<QuickReply> quick_replies { get; set; }
    }

    public class QuickReply
    {
        public string content_type { get; set; }
        public string title { get; set; }
        public string payload { get; set; }
    }


}
