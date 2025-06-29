using CRMS.Domain.Entities;
using System.Linq.Expressions;

namespace CRMS.Business.Services.AttachmentService
{
    public interface IAttachmentService
    {
        public Task<IEnumerable<Attachment>> GetAllAttachmentsAsync();
        public Task<Attachment> GetAttachmentByIdAsync(int id);
        public Task AddAttachmentAsync(Attachment attachment);
        public void UpdateAttachment(Attachment attachment);
        public void DeleteAttachment(Attachment attachment);
        public Task<IEnumerable<Attachment>> FindAttachmentsAsync(Expression<Func<Attachment, bool>> predicate);
    }
}
