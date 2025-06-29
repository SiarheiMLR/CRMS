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
    public class TransactionManager : BaseManager
    {
        public TransactionManager(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync() => await transactionRepository.GetAllAsync();
        public async Task<Transaction> GetTransactionByIdAsync(int id) => await transactionRepository.GetByIdAsync(id);
        public async Task AddTransactionAsync(Transaction transaction)
        {
            await transactionRepository.AddAsync(transaction);
            await SaveChangesAsync();
        }
        public void UpdateTransaction(Transaction transaction)
        {
            transactionRepository.Update(transaction);
            unitOfWork.SaveChanges();
        }
        public void DeleteTransaction(Transaction transaction)
        {
            transactionRepository.Remove(transaction);
            unitOfWork.SaveChanges();
        }
        public async Task<IEnumerable<Transaction>> FindTransactionsAsync(Expression<Func<Transaction, bool>> predicate)
            => await transactionRepository.FindAsync(predicate);
    }
}
