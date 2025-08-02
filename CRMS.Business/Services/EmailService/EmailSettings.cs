using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Business.Services.EmailService
{
    public class EmailSettings
    {
        /// <summary>
        /// SMTP-сервер (hMailServer)
        /// </summary>
        public string SmtpServer { get; set; } = "smtp.bigfirm.by";

        /// <summary>
        /// Порт SMTP (чаще всего 465 для SSL или 587 для StartTLS)
        /// </summary>
        public int SmtpPort { get; set; } = 465;

        /// <summary>
        /// Использовать SSL при подключении
        /// </summary>
        public bool UseSsl { get; set; } = true;

        /// <summary>
        /// Адрес, который видит получатель в поле "От кого"
        /// </summary>
        public string From { get; set; } = "it-support@bigfirm.by";

        /// <summary>
        /// Имя, отображаемое рядом с адресом "От кого"
        /// </summary>
        public string FromDisplayName { get; set; } = "Служба поддержки пользователей CRMS";

        /// <summary>
        /// Реальный SMTP-аккаунт (relay-учетка в hMailServer)
        /// </summary>
        public string Username { get; set; } = "crms-relay@bigfirm.by";

        /// <summary>
        /// Пароль SMTP relay-пользователя
        /// </summary>
        public string Password { get; set; } = "Hp27011984";
    }
}

