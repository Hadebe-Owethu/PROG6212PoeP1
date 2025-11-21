using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgPOEP1.Models
{
    public class ClaimApproval
    {
        [Key]
        public int ApprovalID { get; set; }

        [Required]
        [ForeignKey("Claim")]
        public string ClaimID { get; set; }

        [Required]
        public string ApprovedBy { get; set; }

        // Use string for now; enum optional
        [Required]
        public string Action { get; set; } // Verified, Approved, Rejected

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string Comments { get; set; }

        public virtual Claim Claim { get; set; }
    }
}
