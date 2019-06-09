using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPC.IServiceAgent
{
    public interface IMailSenderSA
    {
        Task SendMailAsync(string senderMail, string senderDisplayName, string senderPassword, string recipientMail, string recipientDisplayName, string subject, string body);
    }
}
