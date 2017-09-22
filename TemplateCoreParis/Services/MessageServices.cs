using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace TemplateCoreParis.Services
{
    // This class is used by the application to send Email and SMS
    // when you turn on two-factor authentication in ASP.NET Identity.
    // For more details see this link https://go.microsoft.com/fwlink/?LinkID=532713
    public class AuthMessageSender : IEmailSender, ISmsSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            // Plug in your email service here to send an email.
            return Task.FromResult(0);
        }

        public static SMSoptions Options = new SMSoptions()
        {
            SMSAccountIdentification = "AC7354ea9b406aed6252047ba36b3c5d01",
            SMSAccountPassword = "47bafd79cfe0ec238f9216b9932fd27a",
            SMSAccountFrom = "+15126452521"
        };

        public Task SendSmsAsync(string number, string message)
        {
            try
            {
                var accountSid = Options.SMSAccountIdentification;
                var authToken = Options.SMSAccountPassword;

                TwilioClient.Init(accountSid, authToken);

                number = "+" + number;

                var msg = MessageResource.Create(
                  to: new PhoneNumber(number),
                  from: new PhoneNumber(Options.SMSAccountFrom),
                  body: message);

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.InnerException);
            }



            return Task.FromResult(0);
        }
    }
}
