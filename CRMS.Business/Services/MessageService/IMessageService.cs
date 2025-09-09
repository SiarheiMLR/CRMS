using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Business.Services.MessageService
{
    public interface IMessageService
    {
        void ShowInfo(string message);
        void ShowError(string message);
        bool ShowConfirmation(string message, string title);
    }
}
