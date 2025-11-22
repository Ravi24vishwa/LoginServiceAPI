using System.ComponentModel.DataAnnotations;

namespace API_Practice.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string MobileNumber { get; set; }

        public string Gender { get; set; }

        public string PasswordHash { get; set; }
    }
}
