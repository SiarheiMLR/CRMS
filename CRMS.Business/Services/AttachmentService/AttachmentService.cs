using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CRMS.Business.Services.AttachmentService
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AttachmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddAttachmentAsync(Attachment attachment)
        {
            await _unitOfWork.Attachments.AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async void DeleteAttachment(Attachment attachment)
        {
            _unitOfWork.Attachments.Remove(attachment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<Attachment>> FindAttachmentsAsync(Expression<Func<Attachment, bool>> predicate)
        {
            return await _unitOfWork.Attachments.GetWhereAsync(predicate);
        }

        public async Task<IEnumerable<Attachment>> GetAllAttachmentsAsync()
        {
            return await _unitOfWork.Attachments.GetAllAsync();
        }

        public async Task<Attachment> GetAttachmentByIdAsync(int id)
        {
            return await _unitOfWork.Attachments.GetByIdAsync(id);
        }

        public async void UpdateAttachment(Attachment attachment)
        {
            _unitOfWork.Attachments.Update(attachment);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
