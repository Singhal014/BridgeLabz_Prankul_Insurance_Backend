using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Models
{
    public class CustomerPolicyModel
    {
        [Required]
        public int PlanId { get; set; }

        [Required]
        public int SchemeId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Maturity period must be at least 1 year")]
        public int MaturityPeriod { get; set; }
    }
}
