namespace ProgPOEP1.Models
{
    public class ClaimApproval
    {
        public string ApprovalID { get; set; } 
        public string ClaimID { get; set; }  
        public string LecturerID { get; set; } 
        public DateTime ApprovalDate { get; set; }
        public string Comments { get; set; }

        // Navigation properties
        public Claim Claim { get; set; }
        public Lecturer Lecturer { get; set; }
    }

}
