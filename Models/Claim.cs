using System;
using System.ComponentModel.DataAnnotations;

namespace ProgPOEP1.Models
{
    public class Claim
    {
        // System-managed fields
        public string ClaimID { get; set; }
        public string ContractorID { get; set; }
        public string DocumentPath { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property (must NOT be required)
        public virtual Lecturer Lecturer { get; set; }

        // Form-bound fields
        [Required(ErrorMessage = "Please select a month")]
        public string Month { get; set; }

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(0.5, 180, ErrorMessage = "Hours worked must be between 0.5 and 180")]
        public decimal HoursWorked { get; set; }

        // Set by controller from session
        public decimal HourlyRate { get; set; }

        public string Notes { get; set; }
    }
}
