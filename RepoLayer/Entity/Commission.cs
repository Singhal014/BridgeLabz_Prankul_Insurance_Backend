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
        public long CommissionId { get; set; }

        [Required]
        public long AgentId { get; set; }

        [Required]
        public long PolicyId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal CommissionAmount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
