using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelLayer.Models
{
    public class CustomerRegistration
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int? AgentID { get; set; }
    }
}
