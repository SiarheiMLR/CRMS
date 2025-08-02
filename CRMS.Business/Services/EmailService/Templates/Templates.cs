using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Business.Services.EmailService.Templates
{
    public static class Templates
    {
        public static string UserCreatedFromAD => @"
        <p>Здравствуйте, {FirstName}!</p>
        <p>Ваша учетная запись в <b>CRMS</b> была создана на основе данных <b>Active Directory</b>.</p>
        <p>
        <b>Логин:</b> {Email}<br/>
        <b>Первоначальный пароль:</b> {Password}
        </p>
        <p>Пожалуйста, смените пароль при первом входе в систему.</p>";

        public static string UserCreatedManual => @"
        <p>Здравствуйте, {FirstName}!</p>
        <p>Ваша учетная запись в <b>CRMS</b> была создана вручную администратором.</p>
        <p>
        <b>Логин:</b> {Email}<br/>
        <b>Первоначальный пароль:</b> {Password}
        </p>
        <p>Пожалуйста, смените пароль при первом входе в систему.</p>";

        public static string AdminNotificationUserCreated => @"
        <p><b>Уведомление для администратора CRMS</b></p>
        <p>Был создан новый пользователь:</p>
        <ul>
        <li><b>Имя:</b> {DisplayName}</li>
        <li><b>Email:</b> {Email}</li>
        <li><b>Пароль:</b> {Password}</li>
        <li><b>Источник:</b> {Source}</li>
        <li><b>Дата создания:</b> {Date}</li>
        </ul>";

        public static string TicketCreated => @"
        <p>Здравствуйте, {UserName}!</p>
        <p>Вы создали тикет в системе CRMS:</p>
        <p>
        <b>Тема:</b> {Subject}<br/>
        <b>Описание:</b><br/>{Description}
        </p>";

        public static string TicketNotification => @"
        <p>Уведомление о тикете в системе CRMS</p>
        <p>
        <b>Пользователь:</b> {UserName}<br/>
        <b>Время создания:</b> {CreatedAt}<br/>
        <b>Категория:</b> {Category}<br/>
        <b>Описание:</b><br/>{Description}
        </p>";

        public static string TicketCompleted => @"
        <p>Тикет завершен.</p>
        <p>
        <b>Пользователь:</b> {UserName}<br/>
        <b>Исполнитель:</b> {Executor}<br/>
        <b>Дата завершения:</b> {CompletedAt}<br/>
        <b>Результат:</b> {Result}<br/>
        </p>";

        //public static string ForgotPassword => @"
        //<p><b>Уведомление для администратора CRMS</b></p>
        //<p>Поступил запрос на восстановление пароля.</p>
        //<ul>
        //<li><b>Email пользователя:</b> {Email}</li>
        //<li><b>Дата запроса:</b> {Date}</li>
        //</ul>
        //<p>Рекомендуется связаться с пользователем и выполнить процедуру восстановления доступа.</p>";
        public static string ForgotPassword => @"
        <!DOCTYPE html>
        <html lang=""ru"">
        <head>
            <meta charset=""UTF-8"">
            <style>
                body {
                    font-family: 'Segoe UI', sans-serif;
                    background-color: #f9f9f9;
                    color: #333;
                    line-height: 1.6;
                }

                .container {
                    background-color: #ffffff;
                    padding: 20px 30px;
                    border-radius: 6px;
                    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.05);
                    max-width: 600px;
                    margin: auto;
                }

                h2 {
                    color: #4a90e2;
                }

                .footer {
                    font-size: 12px;
                    color: #999;
                    margin-top: 30px;
                }
            </style>
        </head>
        <body>
            <div class=""container"">
                <h2>Запрос на восстановление пароля</h2>
                <p>Уважаемый администратор!</p>
                <p>Пользователь <strong>{DisplayName}</strong> (<a href=""mailto:{Email}"">{Email}</a>) сообщил, что забыл свой пароль и запросил восстановление доступа к системе CRMS.</p>

                <p>Пожалуйста, свяжитесь с пользователем для предоставления инструкции по восстановлению или выдаче временного пароля.</p>

                <p>Дата и время запроса: <strong>{RequestDate}</strong></p>

                <p>С уважением,<br>CRMS Notification Service</p>

                <div class=""footer"">
                    Это автоматическое уведомление. Пожалуйста, не отвечайте на это письмо.
                </div>
            </div>
        </body>
        </html>";
    }
}

