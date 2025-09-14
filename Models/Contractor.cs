using System.Security.Claims;

namespace ProgPOEP1.Models
{
    public class Contractor
    {
        public string ContractorID { get; set; }  
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public decimal HourlyRate { get; set; }

        // Navigation property
        public List<Claim> Claims { get; set; }
    }

}
