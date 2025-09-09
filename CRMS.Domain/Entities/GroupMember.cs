using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRMS.Domain.Entities
{
    public class GroupMember
    {
        [Key]
        public int Id { get; set; } // Уникальный идентификатор

        [Required]
        public int UserId { get; set; } // Внешний ключ пользователя
        public User User { get; set; } // Навигационное свойство

        [Required]
        public int GroupId { get; set; } // Внешний ключ группы
        public Group Group { get; set; } // Навигационное свойство

        public DateTime JoinedAt { get; set; } = DateTime.Now; // Дата добавления в группу
    }
}
