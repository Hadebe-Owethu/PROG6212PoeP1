using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ProgPOEP1.Models;
using System.IO;

namespace ProgPOEP1.Controllers
{
    public class LecturerController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var lecturer = HRController.lecturers.FirstOrDefault(l => l.Username == username && l.Password == password && l.IsApproved);
            if (lecturer != null)
            {
                HttpContext.Session.SetString("LecturerID", lecturer.LecturerID);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Message = "Invalid credentials or not approved.";
            return View();
        }

        public IActionResult Dashboard()
        {
            var lecturerId = HttpContext.Session.GetString("LecturerID");
            if (lecturerId == null)
                return RedirectToAction("Login");

            var claims = CoordinatorController.pendingClaims
                .Where(c => c.ContractorID == lecturerId)
                .ToList();

            ViewBag.Claims = claims;
            ViewBag.Message = TempData["Message"];
            return View();
        }

        public IActionResult SubmitClaim()
        {
            var lecturerId = HttpContext.Session.GetString("LecturerID");
            var lecturer = HRController.lecturers.FirstOrDefault(l => l.LecturerID == lecturerId);

            if (lecturer == null)
                return RedirectToAction("Login");

            ViewBag.HourlyRate = lecturer.HourlyRate;
            ViewBag.FullName = lecturer.FullName;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitClaim(string month, int hours, string notes, IFormFile document)
        {
            var lecturerId = HttpContext.Session.GetString("LecturerID");
            var lecturer = HRController.lecturers.FirstOrDefault(l => l.LecturerID == lecturerId);

            if (lecturer == null)
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(month) || hours <= 0 || hours > 180)
            {
                TempData["Message"] = "Invalid input. Hours must be between 1 and 180.";
                return RedirectToAction("SubmitClaim");
            }

            decimal rate = lecturer.HourlyRate;
            string? documentPath = null;

            if (document != null && document.Length > 0)
            {
                var ext = Path.GetExtension(document.FileName).ToLowerInvariant();
                if (!new[] { ".pdf", ".docx", ".xlsx" }.Contains(ext))
                {
                    TempData["Message"] = "Invalid file type.";
                    return RedirectToAction("SubmitClaim");
                }

                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents");
                Directory.CreateDirectory(uploads);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploads, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                document.CopyTo(stream);
                documentPath = "/documents/" + fileName;
            }

            var newClaim = new Claim
            {
                ClaimID = "CLM" + Guid.NewGuid().ToString("N"),
                ContractorID = lecturer.LecturerID,
                Month = month,
                HoursWorked = hours,
                HourlyRate = rate,
                DocumentPath = documentPath ?? "",
                Status = "Pending",
                Notes = notes,
                CreatedAt = DateTime.UtcNow
            };

            CoordinatorController.pendingClaims.Add(newClaim);
            TempData["Message"] = $"Claim {newClaim.ClaimID} submitted.";
            return RedirectToAction("Dashboard");
        }
        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Signup(Lecturer model)
        {
            model.LecturerID = "LECT" + Guid.NewGuid().ToString("N").Substring(0, 5);
            model.IsApproved = false; // Require manual approval
            HRController.lecturers.Add(model);

            ViewBag.Message = "Signup successful. Await approval.";
            return View();
        }

    }
}
