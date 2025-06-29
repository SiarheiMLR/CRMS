using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Services
{
    public interface IWindowService
    {
        void ShowWindow<T>() where T : new(); // Открыть окно
        void CloseWindow<T>(); // Закрыть окно
    }
}
