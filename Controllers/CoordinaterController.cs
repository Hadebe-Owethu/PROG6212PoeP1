using Microsoft.AspNetCore.Mvc;
using ProgPOEP1.Models;

namespace ProgPOEP1.Controllers
{
    public class CoordinatorController : Controller
    {
        private static List<Claim> pendingClaims = new List<Claim>
        {
            new Claim { ClaimID = "CLM001", ContractorID = "CTR001", Month = "September", HoursWorked = 35, HourlyRate = 500, Status = "Pending" },
            new Claim { ClaimID = "CLM002", ContractorID = "CTR002", Month = "October", HoursWorked = 40, HourlyRate = 450, Status = "Pending" }
        };

        public IActionResult ReviewClaims()
        {
            ViewBag.PendingClaims = pendingClaims;
            ViewBag.Message = TempData["Message"];
            return View();
        }

        [HttpPost]
        public IActionResult ApproveClaim(string claimId)
        {
            var claim = pendingClaims.FirstOrDefault(c => c.ClaimID == claimId);
            if (claim != null)
            {
                claim.Status = "Approved";
                TempData["Message"] = $"✅ Claim {claimId} has been approved.";
            }
            return RedirectToAction("ReviewClaims");
        }

        [HttpPost]
        public IActionResult RejectClaim(string claimId)
        {
            var claim = pendingClaims.FirstOrDefault(c => c.ClaimID == claimId);
            if (claim != null)
            {
                claim.Status = "Rejected";
                TempData["Message"] = $"❌ Claim {claimId} has been rejected.";
            }
            return RedirectToAction("ReviewClaims");
        }
    }
}
