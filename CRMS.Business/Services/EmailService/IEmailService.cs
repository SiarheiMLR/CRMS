using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Business.Services.EmailService
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, string? fromName = null, string? fromEmail = null);
        Task SendTemplateAsync(string to, string subject, string template, Dictionary<string, string> parameters);
    }
}
