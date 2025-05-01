using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Models
{
    public class PlanModel
    {
        [Required]
        public string PlanName { get; set; }

        public string Description { get; set; }
    }
}
