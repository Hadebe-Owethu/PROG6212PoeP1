using System;
using System.ComponentModel.DataAnnotations;

namespace ProgPOEP1.Models
{
    public class ClaimReportDocument
    {
        [Key]
        public int DocumentID { get; set; }

        [Required]
        public string ClaimID { get; set; }

        [Required]
        public string DocumentName { get; set; }

        [Required]
        public string FilePath { get; set; }

        public string FileType { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.Now;

        public string UploadedBy { get; set; }

        // Navigation property
        public virtual Claim Claim { get; set; }
    }
}