using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepoLayer.Entity
{
    [Table("Agents")]
    public class Agent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AgentID { get; set; }
        public string FullName {  get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "Agent";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
