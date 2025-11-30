using KCRM.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KCRM.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = null!;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [Required]
        public string Address { get; set; } = null!;
        public int IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // İlişkiler
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<TaskItem>? Tasks { get; set; }
    }
}
