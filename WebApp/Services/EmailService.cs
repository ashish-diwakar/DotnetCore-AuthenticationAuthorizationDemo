using Microsoft.Extensions.Options;
using WebApp.Settings;

namespace WebApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IOptions<SmtpSetting> smtpSetting;

        public EmailService(IOptions<SmtpSetting> smtpSetting)
        {
            this.smtpSetting = smtpSetting;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(this.smtpSetting.Value.FromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            using (var client = new System.Net.Mail.SmtpClient(this.smtpSetting.Value.Host, this.smtpSetting.Value.Port))
            {
                client.UseDefaultCredentials = false;
                client.EnableSsl = true;
                client.Credentials = new System.Net.NetworkCredential(this.smtpSetting.Value.User, this.smtpSetting.Value.Password);
                emailMessage.To.Add(toEmail);
                await client.SendMailAsync(emailMessage);
            }
        }
    }
}
