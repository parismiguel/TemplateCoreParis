using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("VCA", "paris.pantigoso@vcaperu.com"));
            emailMessage.To.Add(new MailboxAddress(email, email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("Html") { Text = message };

            using (var client = new SmtpClient())
            {
                var credentials = new NetworkCredential
                {
                    UserName = "paris.pantigoso@vcaperu.com", // replace with valid value
                    Password = "ParisVCA2016" // replace with valid value
                };

                client.LocalDomain = "gmail.com";
                // check your smtp server setting and amend accordingly:
                await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.Auto).ConfigureAwait(false);
                await client.AuthenticateAsync(credentials);
                await client.SendAsync(emailMessage).ConfigureAwait(false);
                await client.DisconnectAsync(true).ConfigureAwait(false);
            }
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
