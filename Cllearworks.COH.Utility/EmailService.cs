using System;
using System.Net.Mail;

namespace Cllearworks.COH.Utility
{
    public class EmailService
    {
        public static bool SendEmail(string from, string to, string subject, string message)
        {
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(from);
            mailMessage.To.Add(to);
            mailMessage.Subject = subject;
            mailMessage.Body = message;
            mailMessage.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient();
            try
            {
                smtp.Send(mailMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return true;
        }
    }
}
