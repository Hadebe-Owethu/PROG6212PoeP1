using System.ComponentModel.DataAnnotations;

namespace ProgPOEP1.Models
{
    public class Claim
    {
        [Key]
        public string ClaimID { get; set; }

        public string ContractorID { get; set; }
        public Lecturer? Contractor { get; set; }
        public string Month { get; set; }
        public int HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount => HoursWorked * HourlyRate;

        public string DocumentPath { get; set; }
        public string Notes { get; set; }
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
    }
}
