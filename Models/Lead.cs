using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KCRM.Models
{
    public class Lead
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage ="İsim Soyisim alanı zorunludur.")]
        public string FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Address { get; set; } // Adres bilgisi opsiyonel olabilir

        public string? CompanyName { get; set; } // Lead için şirket adı önemli olabilir
        public string? Source { get; set; } // Geldiği kaynak (Google, Referans vb.)

        public int IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // İlişkiler
        // Lead'i sisteme kaydeden kullanıcı (Satış temsilcisi)
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        // Lead ile ilgili görevler ve notlar
        public ICollection<TaskItem>? Tasks { get; set; }
        public ICollection<Notes>? Notes { get; set; }
    }
}
