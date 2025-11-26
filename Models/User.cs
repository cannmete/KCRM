using KCRM.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KCRM.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        [Required]
        [Column(TypeName = "longblob")]
        public byte[] PasswordHash { get; set; }

        [Required]
        [Column(TypeName = "longblob")]
        public byte[] PasswordSalt { get; set; }
        public string Role { get; set; } = "User";  // User - Admin gibi roller için.

        public ICollection<Customer>? Customers { get; set; }
        public ICollection<TaskItem>? Tasks { get; set; }
        public ICollection<Notes> Notes { get; set; } = new List<Notes>();
    }
}
