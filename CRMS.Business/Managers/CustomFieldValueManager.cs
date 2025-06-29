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
    public class CustomFieldValueManager : BaseManager
    {
        public CustomFieldValueManager(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        // Получение всех значений пользовательских полей
        public async Task<IEnumerable<CustomFieldValue>> GetAllCustomFieldValuesAsync() => await customFieldValueRepository.GetAllAsync();

        // Получение значения пользовательского поля по ID
        public async Task<CustomFieldValue> GetCustomFieldValueByIdAsync(int id) => await customFieldValueRepository.GetByIdAsync(id);

        // Добавление нового значения пользовательского поля
        public async Task AddCustomFieldValueAsync(CustomFieldValue customFieldValue)
        {
            await customFieldValueRepository.AddAsync(customFieldValue);
            await SaveChangesAsync();
        }

        // Обновление значения пользовательского поля
        public void UpdateCustomFieldValue(CustomFieldValue customFieldValue)
        {
            customFieldValueRepository.Update(customFieldValue);
            unitOfWork.SaveChanges();
        }

        // Удаление значения пользовательского поля
        public void DeleteCustomFieldValue(CustomFieldValue customFieldValue)
        {
            customFieldValueRepository.Remove(customFieldValue);
            unitOfWork.SaveChanges();
        }

        // Поиск значений пользовательских полей по предикату
        public async Task<IEnumerable<CustomFieldValue>> FindCustomFieldValuesAsync(Expression<Func<CustomFieldValue, bool>> predicate)
            => await customFieldValueRepository.FindAsync(predicate);
    }
}
