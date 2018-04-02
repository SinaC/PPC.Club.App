using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using PPC.IServiceAgent;

namespace PPC.ServiceAgent.MailSender
{
    public class MailSenderSA : IMailSenderSA
    {
        public async Task SendMailAsync(string senderMail, string senderDisplayName, string senderPassword, string recipientMail, string recipientDisplayName, string subject, string body)
        {
            MailAddress fromAddress = new MailAddress(senderMail, senderDisplayName);
            MailAddress toAddress = new MailAddress(recipientMail, recipientMail);
            using (SmtpClient client = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, senderPassword)
            })
            {
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    await client.SendMailAsync(message);
                }
            }
        }
    }
}
