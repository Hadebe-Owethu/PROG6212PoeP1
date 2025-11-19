using Microsoft.AspNetCore.Mvc;
using ProgPOEP1.Models;
using System.Text;

namespace ProgPOEP1.Controllers
{
    public class HRController : Controller
    {
        public static List<Lecturer> lecturers = new List<Lecturer>();

        public IActionResult ManageLecturers()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.Lecturers = lecturers;
            return View();
        }

        [HttpPost]
        public IActionResult AddOrUpdateLecturer(string lecturerId, string fullName, string email, string department, decimal hourlyRate, string username, string password)
        {
            var existing = lecturers.FirstOrDefault(l => l.LecturerID == lecturerId);
            if (existing != null)
            {
                existing.FullName = fullName;
                existing.Email = email;
                existing.Department = department;
                existing.HourlyRate = hourlyRate;
                existing.Username = username;
                existing.Password = password;
                TempData["Message"] = "Lecturer updated.";
            }
            else
            {
                lecturers.Add(new Lecturer
                {
                    LecturerID = lecturerId,
                    FullName = fullName,
                    Email = email,
                    Department = department,
                    HourlyRate = hourlyRate,
                    Username = username,
                    Password = password,
                    IsApproved = false
                });
                TempData["Message"] = "Lecturer added.";
            }

            return RedirectToAction("ManageLecturers");
        }

        [HttpPost]
        public IActionResult ApproveLecturer(string lecturerId)
        {
            var lecturer = lecturers.FirstOrDefault(l => l.LecturerID == lecturerId);
            if (lecturer != null)
            {
                lecturer.IsApproved = true;
                TempData["Message"] = $"Lecturer {lecturerId} approved.";
            }
            return RedirectToAction("ManageLecturers");
        }

        public IActionResult PaymentReport()
        {
            var approvedClaims = CoordinatorController.pendingClaims
                .Where(c => c.Status == "Approved")
                .OrderBy(c => c.ContractorID)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("ClaimID,ContractorID,Month,HoursWorked,HourlyRate,TotalAmount,ApprovedAt");
            foreach (var claim in approvedClaims)
            {
                sb.AppendLine($"{claim.ClaimID},{claim.ContractorID},{claim.Month},{claim.HoursWorked},{claim.HourlyRate},{claim.TotalAmount},{claim.ApprovedAt}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"ApprovedClaims_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }
    }
}
