using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateCoreParis.FacebookChat
{
    public class ContextWatsonFB
    {
        [Key]
        public int ContextWatsonFbId { get; set; }

        public string RecipientId { get; set; }
        public string SenderId { get; set; }

        public string Context { get; set; }

        public DateTime DateTimeUpdated { get; set; }
    }
}
