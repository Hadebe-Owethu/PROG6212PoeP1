using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgPOEP1.Models;
using ProgPOEP1.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ProgPOEP1.Controllers
{
    public class HRController : Controller
    {
        private readonly AppDbContext _context;

        public HRController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> ManageLecturers()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.Lecturers = await _context.Lecturers.OrderBy(l => l.FullName).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateLecturer(string lecturerId, string fullName, string email, string department, decimal hourlyRate, string username, string password)
        {
            var existing = await _context.Lecturers.FirstOrDefaultAsync(l => l.LecturerID == lecturerId);
            if (existing != null)
            {
                existing.FullName = fullName;
                existing.Email = email;
                existing.Department = department;
                existing.HourlyRate = hourlyRate;
                existing.Username = username;
                existing.Password = password;
                _context.Lecturers.Update(existing);
                TempData["Message"] = "Lecturer updated.";
            }
            else
            {
                var newLecturer = new Lecturer
                {
                    LecturerID = lecturerId,
                    FullName = fullName,
                    Email = email,
                    Department = department,
                    HourlyRate = hourlyRate,
                    Username = username,
                    Password = password,
                    IsApproved = false
                };
                await _context.Lecturers.AddAsync(newLecturer);
                TempData["Message"] = "Lecturer added.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("ManageLecturers");
        }

        [HttpPost]
        public async Task<IActionResult> ApproveLecturer(string lecturerId)
        {
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.LecturerID == lecturerId);
            if (lecturer != null)
            {
                lecturer.IsApproved = true;
                _context.Lecturers.Update(lecturer);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Lecturer {lecturerId} approved.";
            }
            return RedirectToAction("ManageLecturers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteLecturer(string lecturerId)
        {
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.LecturerID == lecturerId);
            if (lecturer != null)
            {
                _context.Lecturers.Remove(lecturer);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Lecturer {lecturerId} deleted.";
            }
            return RedirectToAction("ManageLecturers");
        }

        public async Task<IActionResult> PaymentReportPdf()
        {
            var approvedClaims = await _context.Claims
                .Where(c => c.Status == "Approved")
                .OrderBy(c => c.ContractorID)
                .ToListAsync();

            try
            {
                // Optional: log claims for debugging
                foreach (var claim in approvedClaims)
                {
                    Console.WriteLine($"Claim: {claim.ClaimID}, Rate: {claim.HourlyRate}, Hours: {claim.HoursWorked}");
                }

                var document = new ClaimReportDocument(approvedClaims);
                var pdfBytes = document.GeneratePdf();

                return File(pdfBytes, "application/pdf", $"ApprovedClaims_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Message"] = "PDF generation failed: " + ex.Message;
                return RedirectToAction("ManageLecturers");
            }
        }

    }
}
