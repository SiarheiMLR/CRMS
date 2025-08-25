using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Business.Services.EmailService.Templates
{
    public static class Templates
    {
        // Единый стиль оформления шаблонов
        private const string BaseStyles = @"
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f9f9f9;
            color: #333;
            line-height: 1.6;
            margin: 0;
            padding: 20px;
        }
        .container {
            background-color: #ffffff;
            padding: 25px 30px;
            border-radius: 8px;
            box-shadow: 0 3px 10px rgba(0, 0, 0, 0.08);
            max-width: 600px;
            margin: 0 auto;
        }
        h2 {
            color: #4a90e2;
            margin-top: 0;
            border-bottom: 2px solid #f0f0f0;
            padding-bottom: 15px;
        }
        ul {
            padding-left: 20px;
        }
        li {
            margin-bottom: 8px;
        }
        .footer {
            font-size: 12px;
            color: #999;
            margin-top: 30px;
            padding-top: 15px;
            border-top: 1px solid #eee;
        }
        .highlight {
            background-color: #f8f9fa;
            padding: 12px 15px;
            border-radius: 5px;
            margin: 15px 0;
            border-left: 3px solid #4a90e2;
        }
        .no-attachments {
            color: #999;
            font-style: italic;
        }
        .image-placeholder {
            background-color: #f0f0f0;
            border: 1px dashed #ccc;
            padding: 10px;
            color: #999;
            text-align: center;
        }";

        // Метод WrapTemplate для генерации общей структуры
        private static string WrapTemplate(string title, string content)
        {
            return $@"
                    <!DOCTYPE html>
                    <html lang=""ru"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>{title}</title>
                        <style>
                            {BaseStyles}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <h2>{title}</h2>
                            {content}
                            <div class=""footer"">
                                Это автоматическое уведомление. Пожалуйста, не отвечайте на это письмо.
                                <p>С уважением,<br>CRMS Notification Service</p>
                            </div>
                        </div>
                    </body>
                    </html>";
        }

        public static string UserCreatedFromAD => WrapTemplate(
             "Ваша учетная запись в CRMS",
             @"
            <p>Здравствуйте, {FirstName}!</p>
            <p>Ваша учетная запись в системе <b>CRMS</b> была автоматически создана на основе данных <b>Active Directory</b>.</p>
            
            <div class=""highlight"">
                <p><b>Логин:</b> {Email}</p>
                <p><b>Первоначальный пароль:</b> {Password}</p>
            </div>
            
            <p>Для обеспечения безопасности ваших данных, пожалуйста, измените пароль при первом входе в систему.</p>
            <p>Если у вас возникнут вопросы, обратитесь в службу технической поддержки.</p>"
         );

        public static string UserCreatedManual => WrapTemplate(
            "Ваша учетная запись в CRMS",
            @"
            <p>Здравствуйте, {FirstName}!</p>
            <p>Ваша учетная запись в системе <b>CRMS</b> была создана администратором.</p>
            
            <div class=""highlight"">
                <p><b>Логин:</b> {Email}</p>
                <p><b>Первоначальный пароль:</b> {Password}</p>
            </div>
            
            <p>Для обеспечения безопасности ваших данных, пожалуйста, измените пароль при первом входе в систему.</p>
            <p>Если у вас возникнут вопросы, обратитесь в службу технической поддержки.</p>"
        );

        public static string AdminNotificationUserCreated => WrapTemplate(
             "Создан новый пользователь в CRMS",
             @"
            <p>Уведомление для администратора CRMS!</p>
            <p>В системе был зарегистрирован новый пользователь:</p>
            
            <ul>
                <li><b>Имя:</b> {DisplayName}</li>
                <li><b>Email:</b> {Email}</li>
                <li><b>Пароль:</b> {Password}</li>
                <li><b>Источник создания:</b> {Source}</li>
                <li><b>Дата регистрации:</b> {Date}</li>
            </ul>
            
            <p>Пользователю отправлено письмо с данными для входа в систему.</p>"
         );

        // ✅ Новый вариант для пользователя
        public static string TicketCreated(string ticketBodyHtml, string attachmentsHtml) => WrapTemplate(
            "Ваша заявка с номером {TicketNumber} успешно зарегистрирована в системе CRMS!",
            $@"
                <p>Здравствуйте, <b>{{UserName}}</b>!</p>
                <p>Вы успешно создали новую заявку с номером <b>{{TicketNumber}}</b> в системе CRMS <b>{{Created}}</b>!</p>

                <div class=""highlight"">
                    <p><b>Тип заявки:</b> {{Queue}}</p>
                    <p><b>Тема заявки:</b> {{Subject}}</p>                    
                    <p><b>Приоритет:</b> <span style='color: {{PriorityColor}}; font-weight: bold;'>{{Priority}}</span></p>
                </div>

                <h3>Содержание заявки:</h3>
                <div>{ticketBodyHtml}</div>

                
                <h3>Прикрепленные вами файлы:</h3>
                <ul>
                    {attachmentsHtml}
                </ul>
            
            <p>Ваш запрос принят в обработку. Мы уведомим вас об изменениях его статуса.</p>"
        );

        // ✅ Новый вариант для службы поддержки
        public static string TicketCreatedForSupport(string ticketBodyHtml, string attachmentsHtml) => WrapTemplate(
            "В системе CRMS зарегистрирована новая заявка под номером {TicketNumber}!",
            $@"
                <p>В системе CRMS <b>{{Created}}</b> зарегистрирована новая заявка  от пользователя <b>{{UserName}} ({{UserEmail}})</b> </p>

                <div class=""highlight"">
                    <p><b>Номер заявки:</b> {{TicketNumber}}</p>
                    <p><b>Пользователь:</b> {{UserName}} ({{UserEmail}})</p>
                    <p><b>Тип заявки:</b> {{Queue}}</p>
                    <p><b>Тема заявки:</b> {{Subject}}</p>
                    <p><b>Приоритет:</b> <span style='color: {{PriorityColor}}; font-weight: bold;'>{{Priority}}</span></p>
                </div>

                <h3>Содержание заявки:</h3>
                <div>{ticketBodyHtml}</div>

                <h3>Прикрепленные пользователем файлы:</h3>
                <ul>
                    {attachmentsHtml}
                </ul>

            <p>Пожалуйста, примите заявку в работу в установленные сроки.</p>"
        );

        public static string TicketNotification => WrapTemplate(
            "Уведомление о новом тикете",
            @"
            <p>Уважаемый сотрудник службы поддержки!</p>
            <p>В системе CRMS создан новый тикет, требующий вашего внимания:</p>
            
            <ul>
                <li><b>Пользователь:</b> {UserName}</li>
                <li><b>Время создания:</b> {CreatedAt}</li>
                <li><b>Категория:</b> {Category}</li>
                <li><b>Приоритет:</b> <span style='color: {PriorityColor}; font-weight: bold;'>{Priority}</span></li>
                <li><b>Описание:</b><br/>{Description}</li>
            </ul>
            
            <p>Пожалуйста, обработайте запрос в установленные сроки.</p>"
        );

        public static string TicketCompleted => WrapTemplate(
            "Тикет завершен",
            @"
            <p>Здравствуйте, {UserName}!</p>
            <p>Ваш тикет в системе CRMS был успешно завершен.</p>
            
            <ul>
                <li><b>Тема:</b> {Subject}</li>
                <li><b>Исполнитель:</b> {Executor}</li>
                <li><b>Дата завершения:</b> {CompletedAt}</li>
                <li><b>Результат:</b><br/>{Result}</li>
            </ul>
            
            <p>Если решение проблемы вас не удовлетворило, вы можете повторно открыть тикет в течение 3 дней.</p>
            <p>Спасибо за обращение в службу поддержки!</p>"
        );

        public static string ForgotPassword => WrapTemplate(
            "Запрос на восстановление пароля",
            @"
            <p>Уведомление для администратора CRMS!</p>
            <p>Пользователь <strong>{DisplayName}</strong> (<a href=""mailto:{Email}"">{Email}</a>) 
            сообщил, что забыл свой пароль и запросил восстановление доступа к системе CRMS.</p>
            
            <p>Пожалуйста, свяжитесь с пользователем для предоставления инструкции по восстановлению или выдаче временного пароля.</p>
            
            <div class=""highlight"">
                <p><b>Дата и время запроса:</b> {RequestDate}</p>
            </div>"
        );
    }
}

