namespace ProgPOEP1.Models
{
    public class Lecturer
    {
        public string LecturerID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }

        //New fields for HR automation
        public decimal HourlyRate { get; set; }
        public bool IsApproved { get; set; } = false;

        public string? Username { get; set; }
        public string? Password { get; set; }

        // Navigation property
        public List<ClaimApproval> Approvals { get; set; }
    }
}
