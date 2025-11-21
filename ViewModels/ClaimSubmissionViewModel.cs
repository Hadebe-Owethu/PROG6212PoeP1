using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ProgPOEP1.ViewModels
{
    public class ClaimSubmissionViewModel
    {
        [Required(ErrorMessage = "Please select a month")]
        public string Month { get; set; }

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(0.5, 180, ErrorMessage = "Hours worked must be between 0.5 and 180")]
        public decimal HoursWorked { get; set; }

        public string Notes { get; set; }

        // File upload handled separately
        public IFormFile DocumentFile { get; set; }
    }
}
