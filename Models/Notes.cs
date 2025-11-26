using System;
using System.ComponentModel.DataAnnotations;

namespace KCRM.Models
{
    public class Notes
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lütfen içerik giriniz.")]
        public string Content { get; set; }
        [Required]
        public int UserId { get; set;   }
        public User User { get; set; } = null!; // Nullable olamaz.
        public int? CustomerId { get; set; } // Müşteri ID'sini saklamak için
        public Customer? Customer { get; set; } // Müşteri nesnesine navigasyon için
        public int IsDeleted { get; set; }
    }
}
