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
    public class AttachmentManager : BaseManager
    {
        public AttachmentManager(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<IEnumerable<Attachment>> GetAllAttachmentsAsync() => await attachmentRepository.GetAllAsync();
        public async Task<Attachment> GetAttachmentByIdAsync(int id) => await attachmentRepository.GetByIdAsync(id);
        public async Task AddAttachmentAsync(Attachment attachment)
        {
            await attachmentRepository.AddAsync(attachment);
            await SaveChangesAsync();
        }
        public void UpdateAttachment(Attachment attachment)
        {
            attachmentRepository.Update(attachment);
            unitOfWork.SaveChanges();
        }
        public void DeleteAttachment(Attachment attachment)
        {
            attachmentRepository.Remove(attachment);
            unitOfWork.SaveChanges();
        }
        public async Task<IEnumerable<Attachment>> FindAttachmentsAsync(Expression<Func<Attachment, bool>> predicate)
            => await attachmentRepository.FindAsync(predicate);
    }
}
