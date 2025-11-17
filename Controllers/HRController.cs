using Microsoft.AspNetCore.Mvc;
using ProgPOEP1.Models;
using System.Text;

namespace ProgPOEP1.Controllers
{
    public class HRController : Controller
    {
        // View to manage lecturers (placeholder for POE)
        public IActionResult ManageLecturers()
        {
            ViewBag.Message = TempData["Message"];
            return View();
        }

        // Generate a CSV report of approved claims
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
