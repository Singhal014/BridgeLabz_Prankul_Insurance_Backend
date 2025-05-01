using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Models
{
    public class CommissionModel
    {
        [Required]
        public long PolicyId { get; set; }
    }
}
