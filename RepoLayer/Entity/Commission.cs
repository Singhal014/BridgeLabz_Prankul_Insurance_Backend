using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RepoLayer.Entity
{
    [Table("Commissions")]
    public class Commission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CommissionId { get; set; }

        [Required]
        public int AgentId { get; set; }

        [Required]
        public int PolicyId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CommissionAmount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
