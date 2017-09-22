using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemplateCoreParis.FacebookChat.TemplatesFB
{

    public class FbUserInfo
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string profile_pic { get; set; }
        public string locale { get; set; }
        public int timezone { get; set; }
        public string gender { get; set; }
        public Last_Ad_Referral last_ad_referral { get; set; }
    }

    public class Last_Ad_Referral
    {
        public string source { get; set; }
        public string type { get; set; }
        public string ad_id { get; set; }
    }

}
