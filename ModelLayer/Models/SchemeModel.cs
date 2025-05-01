using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Models
{
    public class SchemeModel
    {
        [Required]
        public int PlanId { get; set; }

        [Required]
        public string SchemeName { get; set; }

        public string Details { get; set; }
    }
}
