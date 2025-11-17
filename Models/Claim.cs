public class Claim
{
    public string ClaimID { get; set; }
    public string ContractorID { get; set; }
    public string Month { get; set; }
    public int HoursWorked { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal TotalAmount => HoursWorked * HourlyRate;
    public string DocumentPath { get; set; }
    public string Status { get; set; } = "Pending";

    //New fields for Part 3 automation
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string Notes { get; set; }
}
