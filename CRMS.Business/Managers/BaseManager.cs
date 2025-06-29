using CRMS.Domain.Interfaces;
using CRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Group = CRMS.Domain.Entities.Group;

namespace CRMS.Business.Managers
{
    public class BaseManager
    {
        // Поля-объекты
        protected readonly IUnitOfWork _unitOfWork;

        // Конструктор
        public BaseManager(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }

        // Метод для сохранения изменений в базе
        public async Task SaveChangesAsync()
        {
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
