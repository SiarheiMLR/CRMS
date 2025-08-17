using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMS.Domain.Entities
{
    public class Attachment
    {
        public int Id { get; set; }

        [Required]
        public int TicketId { get; set; }

        [ForeignKey(nameof(TicketId))]
        public Ticket Ticket { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [MaxLength(255)]
        public string ContentType { get; set; }

        /// <summary>
        /// Фактические бинарные данные файла.
        /// </summary>
        [Required]
        public byte[] FileData { get; set; }

        /// <summary>
        /// Размер файла в байтах.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Дата и время загрузки вложения.
        /// </summary>
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Пользователь, который загрузил файл.
        /// </summary>
        public int UploadedById { get; set; }

        [ForeignKey(nameof(UploadedById))]
        public User UploadedBy { get; set; }
    }
}
