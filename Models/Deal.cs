using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KCRM.Models
{
    // Satış Süreci Aşamaları
    public enum DealStage
    {
        [Display(Name = "Yeni Fırsat")]
        New = 0,

        [Display(Name = "Görüşülüyor")]
        Qualification = 1,

        [Display(Name = "Teklif Verildi")]
        Proposal = 2,

        [Display(Name = "Pazarlık")]
        Negotiation = 3,

        [Display(Name = "Kazanıldı")]
        Won = 4,

        [Display(Name = "Kaybedildi")]
        Lost = 5
    }

    public class Deal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Fırsat başlığı zorunludur.")]
        [Display(Name = "Fırsat Başlığı")]
        public string Title { get; set; } = null!;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        // Parasal değer için decimal en doğrusudur
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tahmini Tutar")]
        public decimal Amount { get; set; }

        [Display(Name = "Aşama")]
        public DealStage Stage { get; set; } = DealStage.New;

        [Display(Name = "Tahmini Kapanış")]
        [DataType(DataType.Date)]
        public DateTime? ClosingDate { get; set; }

        public int IsDeleted { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- İLİŞKİLER ---

        // Hangi Müşteri?
        [ForeignKey("Customer")]
        [Display(Name = "Müşteri")]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // Hangi Satışçı (User)?
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}