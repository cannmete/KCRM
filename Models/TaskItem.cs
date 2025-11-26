using KCRM.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KCRM.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int IsDeleted { get; set; }

        public bool IsCompleted { get; set; } = false;

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
    }
}
