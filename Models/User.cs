using KCRM.Models;
using System.ComponentModel.DataAnnotations;

namespace KCRM.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required, MaxLength(50)] 
        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        [Required]
        public byte[] PasswordHash { get; set; }

        [Required]
        public byte[] PasswordSalt { get; set; }
        public string Role { get; set; } = "User";  // User - Admin gibi roller için.

        public ICollection<Customer>? Customers { get; set; }
        public ICollection<TaskItem>? Tasks { get; set; }
    }
}
