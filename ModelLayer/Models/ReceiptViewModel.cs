namespace ModelLayer.ViewModels
{
    public class ReceiptViewModel
    {
        public int PaymentId { get; set; }
        public string CustomerName { get; set; }
        public string Email { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // Additional properties for file content and file name for download
        public byte[] FileContent { get; set; }  // To hold the generated file content (e.g., PDF as byte array)
        public string FileName { get; set; }     // To specify the file name when downloading
    }
}
