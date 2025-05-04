using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RepoLayer.Entity
{
    [Table("Policies")]
    public class Policy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PolicyId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int PlanId { get; set; }

        [Required]
        public int SchemeId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PremiumAmount { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Maturity period must be at least 1 year")]
        public int MaturityPeriod { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [ForeignKey("PlanId")]
        public InsurancePlan InsurancePlan { get; set; }

        [ForeignKey("SchemeId")]
        public Scheme Scheme { get; set; }
    }
}