namespace ProgPOEP1.Models
{
    public class Claim
    {
        public string ClaimID { get; set; }
        public string ContractorID { get; set; }
        public string Month { get; set; }
        public int HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount => HoursWorked * HourlyRate;
        public string Status { get; set; }  // "Pending", "Approved", "Rejected"
        public string DocumentPath { get; set; }

    }
}
