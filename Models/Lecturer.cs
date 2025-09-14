namespace ProgPOEP1.Models
{
    public class Lecturer
    {
        public string LecturerID { get; set; } 
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }

        // Navigation property
        public List<ClaimApproval> Approvals { get; set; }
    }

}
