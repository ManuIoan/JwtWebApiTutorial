using System.ComponentModel.DataAnnotations;

namespace JwtWebApiTutorial.Models
{
    public class Addresses
    {
        public int id { get; set; }
        [StringLength(100)]
        public string Address { get; set; } = string.Empty;
        public int UATiD { get; set; }
        public UAT? UAT { get; set; }
    }
}
