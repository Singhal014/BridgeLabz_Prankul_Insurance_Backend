using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Commission
{
    [Key]
    public int CommissionId { get; set; }

    [Required]
    public int AgentId { get; set; }

    [Required]
    public int PolicyId { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal CommissionAmount { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsPaid { get; set; } = false;

    public DateTime? PaidDate { get; set; }
}
