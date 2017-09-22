namespace TemplateCoreParis.FacebookChat.TemplatesFB.TextFB
{

    public class TextTemplate
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
    }

    public class ActionTemplate
    {
        public Recipient recipient { get; set; }
        public string sender_action { get; set; }
    }
}