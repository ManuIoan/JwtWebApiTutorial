using System.ComponentModel.DataAnnotations;

namespace JwtWebApiTutorial.Models
{
    public class UAT
    {
        public int id { get; set; }
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
