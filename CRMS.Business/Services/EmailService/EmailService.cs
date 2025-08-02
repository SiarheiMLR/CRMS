using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace CRMS.Business.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(EmailSettings settings)
        {
            _settings = settings;
        }

        public async Task SendTemplateAsync(string to, string subject, string template, Dictionary<string, string> parameters)
        {
            string body = ApplyTemplate(template, parameters);
            await SendEmailAsync(to, subject, body, _settings.FromDisplayName, _settings.From);
        }

        public async Task SendEmailAsync(string to, string subject, string body, string? fromName = null, string? fromEmail = null)
        {
            var message = new MimeMessage();

            // Устанавливаем отображаемое имя и подменяемый адрес (it-support@bigfirm.by)
            message.From.Add(new MailboxAddress(fromName ?? "CRMS Support", fromEmail ?? _settings.From));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();

            // ✅ Игнорируем проверку сертификата
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(_settings.Username, _settings.Password); // crms-relay@bigfirm.by
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private static string ApplyTemplate(string template, Dictionary<string, string> parameters)
        {
            foreach (var pair in parameters)
            {
                template = template.Replace("{" + pair.Key + "}", pair.Value);
            }
            return template;
        }
    }
}