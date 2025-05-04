using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Models
{
    public class CommissionModel
    {
        [Required]
        public int PolicyId { get; set; }
    }
}
