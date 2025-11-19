using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProgPOEP1.Models;
using ProgPOEP1.Data;
using System.IO;

namespace ProgPOEP1.Controllers
{
    public class LecturerController : Controller
    {
        private readonly AppDbContext _context;

        public LecturerController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var lecturer = await _context.Lecturers
                .FirstOrDefaultAsync(l => l.Username == username && l.Password == password && l.IsApproved);

            if (lecturer != null)
            {
                HttpContext.Session.SetString("LecturerID", lecturer.LecturerID);
                HttpContext.Session.SetString("LecturerName", lecturer.FullName);
                return RedirectToAction("Dashboard");
            }

            ViewBag.Message = "Invalid credentials or not approved.";
            return View();
        }

        // Filter support: optional statusFilter query parameter
        public async Task<IActionResult> Dashboard(string? statusFilter = null)
        {
            var lecturerId = HttpContext.Session.GetString("LecturerID");
            if (string.IsNullOrEmpty(lecturerId))
                return RedirectToAction("Login");

            var query = _context.Claims
                .Where(c => c.ContractorID == lecturerId);

            if (!string.IsNullOrWhiteSpace(statusFilter))
                query = query.Where(c => c.Status == statusFilter);

            var claims = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Claims = claims;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.Message = TempData["Message"];
            ViewBag.LecturerName = HttpContext.Session.GetString("LecturerName");
            return View("ViewClaims");
        }

        public async Task<IActionResult> SubmitClaim()
        {
            var lecturerId = HttpContext.Session.GetString("LecturerID");
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.LecturerID == lecturerId);

            if (lecturer == null)
                return RedirectToAction("Login");

            ViewBag.HourlyRate = lecturer.HourlyRate;
            ViewBag.FullName = lecturer.FullName;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(string month, int hours, string notes, IFormFile? document)
        {
            var lecturerId = HttpContext.Session.GetString("LecturerID");
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.LecturerID == lecturerId);

            if (lecturer == null)
                return RedirectToAction("Login");

            if (string.IsNullOrWhiteSpace(month) || hours <= 0 || hours > 180)
            {
                TempData["Message"] = "Invalid input. Hours must be between 1 and 180.";
                return RedirectToAction("SubmitClaim");
            }

            decimal rate = lecturer.HourlyRate;
            string? documentPath = null;

            if (document is { Length: > 0 })
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
                await document.CopyToAsync(stream);
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
                CreatedAt = DateTime.UtcNow,
                VerifiedAt = null,
                ApprovedAt = null,
                RejectedAt = null
            };

            await _context.Claims.AddAsync(newClaim);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Claim {newClaim.ClaimID} submitted.";
            return RedirectToAction("Dashboard");
        }

        // Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
