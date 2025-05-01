namespace ModelLayer.Models
{
    public class PaymentViewModel
    {
        public long PaymentId { get; set; }
        public long PolicyId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PolicyStatus { get; set; }
        public decimal PremiumAmount { get; set; }
    }
}
