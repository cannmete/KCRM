using System;
using System.ComponentModel.DataAnnotations;

namespace KCRM.Models
{
    public class Notes
    {
        public int Id { get; set; }

        [StringLength(500)]
        [Required(ErrorMessage = "Lütfen içerik giriniz.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Lütfen UserId belirtiniz.")]
        public int UserId { get; set; }

        public int IsDeleted { get; set; }
    }
}
