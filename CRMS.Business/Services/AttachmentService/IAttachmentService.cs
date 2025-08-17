using CRMS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace CRMS.Business.Services.AttachmentService
{
    public interface IAttachmentService
    {
        Task<IEnumerable<Attachment>> GetAllAttachmentsAsync();
        Task<Attachment?> GetAttachmentByIdAsync(int id);
        Task AddAttachmentAsync(Attachment attachment);
        Task AddAttachmentFromFileAsync(IFormFile file, int ticketId);
        Task<byte[]?> DownloadAttachmentAsync(int id);
        Task UpdateAttachmentAsync(Attachment attachment);
        Task DeleteAttachmentAsync(Attachment attachment);
        Task<IEnumerable<Attachment>> FindAttachmentsAsync(Expression<Func<Attachment, bool>> predicate);
    }
}
