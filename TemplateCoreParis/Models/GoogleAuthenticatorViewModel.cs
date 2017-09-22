using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateCoreParis.Models
{
   
    public class GoogleAuthenticatorViewModel
    {
        public string SecretKey { get; set; }
        public string BarcodeUrl { get; set; }
        public string Token { get; set; }

        public string Provider { get; set; }
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        public bool RememberMe { get; set; }
        public bool RememberBrowser { get; set; }

    }
}
