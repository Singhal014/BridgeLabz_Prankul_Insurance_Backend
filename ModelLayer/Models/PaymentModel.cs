using System.ComponentModel.DataAnnotations;

namespace ModelLayer.Models
{
    public class PaymentModel
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int PolicyId { get; set; }
    }
}
