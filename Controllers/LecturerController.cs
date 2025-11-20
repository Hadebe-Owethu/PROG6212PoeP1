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

        // GET: Login page
        public IActionResult Login()
        {
            return View();
        }

        // POST: Login
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

        // Lecturer Dashboard with optional status filter
        public async Task<IActionResult> Dashboard(string? statusFilter = null)
        {
            var lecturerId = HttpContext.Session.GetString("LecturerID");
            if (string.IsNullOrEmpty(lecturerId))
                return RedirectToAction("Login", "Account");


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

            // ✅ Always return Dashboard.cshtml
            return View("Dashboard");
        }

        // GET: Submit Claim
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
        public async Task<IActionResult> SubmitClaim(string month, int hours, string notes, IFormFile document)
        {
            var lecturerId = HttpContext.Session.GetString("LecturerID");
            var hourlyRate = HttpContext.Session.GetString("HourlyRate");

            if (string.IsNullOrEmpty(lecturerId) || string.IsNullOrEmpty(hourlyRate))
            {
                TempData["Message"] = "Session expired. Please log in again.";
                return RedirectToAction("Login", "Account");

            }

            if (hours < 1 || hours > 180)
            {
                TempData["Message"] = "Hours must be between 1 and 180.";
                return RedirectToAction("Dashboard");
            }

            string filePath = null;
            if (document != null && document.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(document.FileName)}";
                filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await document.CopyToAsync(stream);
                }
            }

            var newClaim = new Claim
            {
                ClaimID = Guid.NewGuid().ToString(),
                ContractorID = lecturerId,
                Month = month,
                HoursWorked = hours,
                HourlyRate = decimal.Parse(hourlyRate),
                Notes = notes,
                DocumentPath = filePath,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                await _context.Claims.AddAsync(newClaim);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Claim submitted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error submitting claim: " + ex.Message;
            }

            return RedirectToAction("Dashboard");
        }


        // POST: Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");

        }
    }
}
