using KCRM.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KCRM.Models
{
    // 1. ÖNCELİK SEÇENEKLERİ (ENUM)
    public enum TaskPriority
    {
        Düşük = 0,
        Orta = 1,
        Yüksek = 2,
        Acil = 3
    }

    // 2. DURUM SEÇENEKLERİ (ENUM)
    public enum TaskStatus
    {
        [Display(Name ="Bekliyor")]
        Bekliyor = 0,
        [Display(Name = "İşlemde")]
        Islemde = 1,
        [Display(Name = "Tamamlandı")]
        Tamamlandi = 2,
        [Display(Name = "İptal")]
        Iptal = 3
    }
    public class TaskItem
    {

        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int IsDeleted { get; set; }

        public TaskPriority Priority { get; set; } = TaskPriority.Orta;
        public TaskStatus Status { get; set; } = TaskStatus.Bekliyor;

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // İlişkiler
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; } = null!; // Nullable olamaz.

        [ForeignKey("Customer")]
        public int? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [ForeignKey("Lead")]
        public int? LeadId { get; set; }
        public Lead? Lead { get; set; }
    }
}
