namespace ProgPOEP1.Models
{
    public class Claim
    {
        public string ClaimID { get; set; }  
        public string ContractorID { get; set; } 
        public string Month { get; set; }
        public int HoursWorked { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }  
        public DateTime SubmissionDate { get; set; }

        // Navigation properties
        public Contractor Contractor { get; set; }
        public ClaimApproval ClaimApproval { get; set; }
    }

}
