using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateCoreParis.Models
{
    public class SecretAuthViewModel
    {
        [Display(Name = "Pregunta secreta")]
        public string SecretQuestion { get; set; }

        [Display(Name = "Respuesta secreta")]
        public string SecretResponse { get; set; }

        [Display(Name = "Respuesta")]
        public string Token { get; set; }

        public string Provider { get; set; }
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        public bool RememberMe { get; set; }
        public bool RememberBrowser { get; set; }

        public string Response { get; set; }


    }
}
