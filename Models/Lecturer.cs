using System.ComponentModel.DataAnnotations;
namespace ProgPOEP1.Models
{
    public class Lecturer
    {
        [Key]
        public string LecturerID { get; set; }

        public string FullName { get; set; }
        public string Email { get; set; }
        public string Department { get; set; }
        public decimal HourlyRate { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public bool IsApproved { get; set; }
    }
}