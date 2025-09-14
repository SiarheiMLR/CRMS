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
            
            <p style=""color: red; font-weight: bold;"">
                Для обеспечения безопасности ваших данных, пожалуйста, запросите у администратора CRMS установку надежного пароля!
            </p>
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
            
            <p style=""color: red; font-weight: bold;"">
                Для обеспечения безопасности ваших данных, пожалуйста, запросите у администратора CRMS установку надежного пароля!
            </p>
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
            
            <p>Пользователю отправлено письмо с данными для авторизации в системе!</p>"
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

        //public static string TicketNotification => WrapTemplate(
        //    "Уведомление о новом тикете",
        //    @"
        //    <p>Уважаемый сотрудник службы поддержки!</p>
        //    <p>В системе CRMS создан новый тикет, требующий вашего внимания:</p>

        //    <ul>
        //        <li><b>Пользователь:</b> {UserName}</li>
        //        <li><b>Время создания:</b> {CreatedAt}</li>
        //        <li><b>Категория:</b> {Category}</li>
        //        <li><b>Приоритет:</b> <span style='color: {PriorityColor}; font-weight: bold;'>{Priority}</span></li>
        //        <li><b>Описание:</b><br/>{Description}</li>
        //    </ul>

        //    <p>Пожалуйста, обработайте запрос в установленные сроки.</p>"
        //);

        //public static string TicketCompleted => WrapTemplate(
        //    "Тикет завершен",
        //    @"
        //    <p>Здравствуйте, {UserName}!</p>
        //    <p>Ваш тикет в системе CRMS был успешно завершен.</p>

        //    <ul>
        //        <li><b>Тема:</b> {Subject}</li>
        //        <li><b>Исполнитель:</b> {Executor}</li>
        //        <li><b>Дата завершения:</b> {CompletedAt}</li>
        //        <li><b>Результат:</b><br/>{Result}</li>
        //    </ul>

        //    <p>Если решение проблемы вас не удовлетворило, вы можете повторно открыть тикет в течение 3 дней.</p>
        //    <p>Спасибо за обращение в службу поддержки!</p>"
        //);

        // Обновленные методы для уведомлений о тикетах
        public static string TicketNotification => WrapTemplate(
            "Новая заявка требует вашего внимания",
            @"
                <p>Уважаемый сотрудник службы поддержки!</p>
                <p>В системе CRMS создана новая заявка, требующая вашего внимания:</p>
    
                <div class=""highlight"">
                    <p><b>Номер заявки:</b> {TicketNumber}</p>
                    <p><b>Пользователь:</b> {UserName}</p>
                    <p><b>Время создания:</b> {CreatedAt}</p>
                    <p><b>Тип запроса:</b> {Category}</p>
                    <p><b>Приоритет:</b> <span style='color: {PriorityColor}; font-weight: bold;'>{Priority}</span></p>
                </div>
    
                <h3>Описание проблемы:</h3>
                <div class=""highlight"">
                    {Description}
                </div>
    
            <p>Пожалуйста, обработайте запрос в соответствии с установленными сроками SLA.</p>"
        );

        public static string TicketCompleted => WrapTemplate(
            "Заявка #{TicketNumber} завершена",
            @"
                <p>Здравствуйте, {UserName}!</p>
                <p>Ваша заявка в системе CRMS была успешно завершена.</p>
    
                <div class=""highlight"">
                    <p><b>Номер заявки:</b> {TicketNumber}</p>
                    <p><b>Тема:</b> {Subject}</p>
                    <p><b>Исполнитель:</b> {Executor}</p>
                    <p><b>Дата завершения:</b> {CompletedAt}</p>
                </div>
    
                <h3>Результат работы:</h3>
                <div class=""highlight"">
                    {Result}
                </div>
    
                <p>Если решение проблемы вас не удовлетворило, вы можете повторно открыть заявку в течение 3 дней через систему CRMS.</p>
            <p>Благодарим за обращение в службу поддержки!</p>"
        );

        // 1. Методы для уведомлений об изменении статуса заявки
        public static string TicketStatusChangedToUser => WrapTemplate(
            "Статус вашей заявки #{TicketNumber} изменен",
            @"
                <p>Здравствуйте, {UserName}!</p>
                <p>Статус вашей заявки в системе CRMS был изменен.</p>
    
                <div class=""highlight"">
                    <p><b>Номер заявки:</b> {TicketNumber}</p>
                    <p><b>Тема:</b> {Subject}</p>
                    <p><b>Новый статус:</b> <span style='color: {StatusColor}; font-weight: bold;'>{Status}</span></p>
                    <p><b>Исполнитель:</b> {Executor}</p>
                    <p><b>Дата изменения:</b> {ChangedAt}</p>
                </div>
    
                <h3>Комментарий исполнителя:</h3>
                <div class=""highlight"">
                    {Comment}
                </div>
    
            <p>Вы можете отслеживать статус заявки в личном кабинете системы CRMS.</p>"
        );

        public static string TicketStatusChangedToSupport => WrapTemplate(
            "Статус заявки #{TicketNumber} изменен на {Status}",
            @"
                <p>Уведомление для сотрудников службы поддержки!</p>
                <p>Статус заявки был изменен:</p>
    
                <div class=""highlight"">
                    <p><b>Номер заявки:</b> {TicketNumber}</p>
                    <p><b>Пользователь:</b> {UserName} ({UserEmail})</p>
                    <p><b>Тема:</b> {Subject}</p>
                    <p><b>Тип запроса:</b> {Queue}</p>
                    <p><b>Старый статус:</b> {OldStatus}</p>
                    <p><b>Новый статус:</b> <span style='color: {StatusColor}; font-weight: bold;'>{Status}</span></p>
                    <p><b>Изменил:</b> {ChangedBy}</p>
                    <p><b>Дата изменения:</b> {ChangedAt}</p>
                </div>
    
                <h3>Комментарий:</h3>
                <div class=""highlight"">
                    {Comment}
                </div>
    
            <p>Заявка требует дальнейших действий в соответствии с новым статусом.</p>"
        );

        // 2. Методы для уведомлений о возобновлении заявки
        public static string TicketReopenedToUser => WrapTemplate(
            "Ваша заявка #{TicketNumber} возобновлена",
            @"
                <p>Здравствуйте, {UserName}!</p>
                <p>Ваша ранее закрытая заявка в системе CRMS была возобновлена.</p>
    
                <div class=""highlight"">
                    <p><b>Номер заявки:</b> {TicketNumber}</p>
                    <p><b>Тема:</b> {Subject}</p>
                    <p><b>Исполнитель:</b> {Executor}</p>
                    <p><b>Дата возобновления:</b> {ReopenedAt}</p>
                </div>
    
                <h3>Причина возобновления:</h3>
                <div class=""highlight"">
                    {Reason}
                </div>
    
            <p>Заявка снова находится в работе. Мы уведомим вас о дальнейших изменениях.</p>"
        );

        public static string TicketReopenedToSupport => WrapTemplate(
            "Заявка #{TicketNumber} возобновлена пользователем",
            @"
                <p>Уведомление для сотрудников службы поддержки!</p>
                <p>Заявка была возобновлена пользователем:</p>
    
                <div class=""highlight"">
                    <p><b>Номер заявки:</b> {TicketNumber}</p>
                    <p><b>Пользователь:</b> {UserName} ({UserEmail})</p>
                    <p><b>Тема:</b> {Subject}</p>
                    <p><b>Тип запроса:</b> {Queue}</p>
                    <p><b>Изначальный исполнитель:</b> {OriginalExecutor}</p>
                    <p><b>Дата возобновления:</b> {ReopenedAt}</p>
                </div>
    
                <h3>Причина возобновления:</h3>
                <div class=""highlight"">
                    {Reason}
                </div>
    
            <p>Заявка требует назначения исполнителя и возобновления работы.</p>"
        );

        public static string TicketReopenedToAdmin => WrapTemplate(
            "Заявка #{TicketNumber} возобновлена сотрудником",
            @"
                <p>Уведомление для администратора и ответственных лиц!</p>
                <p>Заявка была возобновлена сотрудником:</p>
    
                <div class=""highlight"">
                    <p><b>Номер заявки:</b> {TicketNumber}</p>
                    <p><b>Пользователь:</b> {UserName} ({UserEmail})</p>
                    <p><b>Тема:</b> {Subject}</p>
                    <p><b>Тип запроса:</b> {Queue}</p>
                    <p><b>Сотрудник, возобновивший заявку:</b> {ReopenedBy}</p>
                    <p><b>Дата возобновления:</b> {ReopenedAt}</p>
                </div>
    
                <h3>Причина возобновления:</h3>
                <div class=""highlight"">
                    {Reason}
                </div>
    
            <p>Заявка требует внимания и возобновления работы.</p>"
        );

        // 3. Методы для уведомлений о добавлении комментария
        public static string TicketCommentAddedToUser => WrapTemplate(
            "Добавлен новый комментарий к вашей заявке #{TicketNumber}",
            @"
                <p>Здравствуйте, {UserName}!</p>
                <p>К вашей заявке в системе CRMS добавлен новый комментарий.</p>
    
                <div class=""highlight"">
                    <p><b>Номер заявки:</b> {TicketNumber}</p>
                    <p><b>Тема:</b> {Subject}</p>
                    <p><b>Автор комментария:</b> {CommentAuthor}</p>
                    <p><b>Дата комментария:</b> {CommentDate}</p>
                </div>
    
                <h3>Текст комментария:</h3>
                <div class=""highlight"">
                    {CommentText}
                </div>
    
            <p>Вы можете просмотреть все комментарии к заявке в личном кабинете системы CRMS.</p>"
        );

        public static string TicketCommentAddedToSupport => WrapTemplate(
            "Добавлен комментарий к заявке #{TicketNumber}",
            @"
                <p>Уведомление для сотрудников службы поддержки!</p>
                <p>К заявке добавлен новый комментарий:</p>
    
                <div class=""highlight"">
                    <p><b>Номер заявки:</b> {TicketNumber}</p>
                    <p><b>Пользователь:</b> {UserName} ({UserEmail})</p>
                    <p><b>Тема:</b> {Subject}</p>
                    <p><b>Тип запроса:</b> {Queue}</p>
                    <p><b>Автор комментария:</b> {CommentAuthor}</p>
                    <p><b>Дата комментария:</b> {CommentDate}</p>
                </div>
    
                <h3>Текст комментария:</h3>
                <div class=""highlight"">
                    {CommentText}
                </div>
    
            <p>Комментарий требует внимания и, возможно, ответных действий.</p>"
        );

        public static string TicketCommentAddedToAuthor => WrapTemplate(
            "Ваш комментарий к заявке #{TicketNumber} добавлен",
            @"
                <p>Уважаемый(ая) {CommentAuthor}!</p>
                <p>Ваш комментарий к заявке в системе CRMS был успешно добавлен.</p>
    
                <div class=""highlight"">
                    <p><b>Номер заявки:</b> {TicketNumber}</p>
                    <p><b>Тема:</b> {Subject}</p>
                    <p><b>Пользователь:</b> {UserName}</p>
                    <p><b>Дата добавления:</b> {CommentDate}</p>
                </div>
    
                <h3>Текст вашего комментария:</h3>
                <div class=""highlight"">
                    {CommentText}
                </div>
    
            <p>Комментарий будет виден всем участникам заявки и может помочь в решении проблемы.</p>"
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

