using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProgPOEP1.Models
{
    public class Lecturer
    {
        [Key]
        public string LecturerID { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Department { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal HourlyRate { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public bool IsApproved { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property for claims
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

    }
}