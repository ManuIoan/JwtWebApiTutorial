using System.ComponentModel.DataAnnotations;

namespace JwtWebApiTutorial.Models
{
    public class Accounts
    {
        public int id { get; set; }
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;
        [StringLength(50)]
        public string Password { get; set; } = string.Empty;
        [StringLength(100)]
        public string Address { get; set; } = string.Empty;
        public int AddressId { get; set; }
        public Addresses? Addresses { get; set; }
    }
}
