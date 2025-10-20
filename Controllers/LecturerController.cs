using Microsoft.AspNetCore.Mvc;
using ProgPOEP1.Models;

namespace ProgPOEP1.Controllers
{
    public class LecturerController : Controller
    {
        public IActionResult Dashboard()
        {
            var claims = CoordinatorController.pendingClaims
                .Where(c => c.ContractorID == "LECT001")
                .ToList();

            ViewBag.Claims = claims;
            return View();
        }

        public IActionResult SubmitClaim()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubmitClaim(string month, int hours, decimal rate, string notes, IFormFile document)
        {
            if (string.IsNullOrEmpty(month) || hours <= 0 || rate <= 0 || document == null)
            {
                ViewBag.Message = "error";
                return View();
            }

            var fileName = document.FileName;
            var documentPath = "/documents/" + fileName;

            var newClaim = new Claim
            {
                ClaimID = "CLM" + Guid.NewGuid().ToString("N"),
                ContractorID = "LECT001",
                Month = month,
                HoursWorked = hours,
                HourlyRate = rate,
                DocumentPath = documentPath,
                Status = "Pending"
            };

            CoordinatorController.pendingClaims.Add(newClaim);
            TempData["Message"] = $"Claim {newClaim.ClaimID} submitted.";
            return RedirectToAction("ReviewClaims", "Coordinator");
        }
    }
}
