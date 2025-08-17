using CRMS.Domain.Entities;
using CRMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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

        public async Task DeleteAttachmentAsync(Attachment attachment)
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

        public async Task<Attachment?> GetAttachmentByIdAsync(int id)
        {
            return await _unitOfWork.Attachments.GetByIdAsync(id);
        }

        public async Task UpdateAttachmentAsync(Attachment attachment)
        {
            _unitOfWork.Attachments.Update(attachment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AddAttachmentFromFileAsync(IFormFile file, int ticketId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Файл пустой");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var attachment = new Attachment
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                FileData = ms.ToArray(),
                TicketId = ticketId
            };

            await _unitOfWork.Attachments.AddAsync(attachment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<byte[]?> DownloadAttachmentAsync(int id)
        {
            var attachment = await _unitOfWork.Attachments.GetByIdAsync(id);
            return attachment?.FileData;
        }
    }
}
