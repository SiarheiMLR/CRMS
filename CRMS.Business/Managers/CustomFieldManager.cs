using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Business.Managers
{
    public class CustomFieldManager : BaseManager
    {
        public CustomFieldManager(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        // Получение всех пользовательских полей
        public async Task<IEnumerable<CustomField>> GetAllCustomFieldsAsync() => await customFieldRepository.GetAllAsync();

        // Получение пользовательского поля по ID
        public async Task<CustomField> GetCustomFieldByIdAsync(int id) => await customFieldRepository.GetByIdAsync(id);

        // Добавление нового пользовательского поля
        public async Task AddCustomFieldAsync(CustomField customField)
        {
            await customFieldRepository.AddAsync(customField);
            await SaveChangesAsync();
        }

        // Обновление пользовательского поля
        public void UpdateCustomField(CustomField customField)
        {
            customFieldRepository.Update(customField);
            unitOfWork.SaveChanges();
        }

        // Удаление пользовательского поля
        public void DeleteCustomField(CustomField customField)
        {
            customFieldRepository.Remove(customField);
            unitOfWork.SaveChanges();
        }

        // Поиск пользовательских полей по предикату
        public async Task<IEnumerable<CustomField>> FindCustomFieldsAsync(Expression<Func<CustomField, bool>> predicate)
            => await customFieldRepository.FindAsync(predicate);
    }
}
