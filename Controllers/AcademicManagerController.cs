using Microsoft.AspNetCore.Mvc;
using ProgPOEP1.Models;

namespace ProgPOEP1.Controllers
{
    public class AcademicManagerController : Controller
    {
        private static List<Claim> pendingClaims = CoordinatorController.pendingClaims;

        public IActionResult AcademicManagerView()
        {
            ViewBag.PendingClaims = pendingClaims;
            return View();
        }

        [HttpPost]
        public IActionResult ApproveClaim(string claimId)
        {
            var claim = pendingClaims.FirstOrDefault(c => c.ClaimID == claimId);
            if (claim != null)
            {
                claim.Status = "Approved";
                TempData["Message"] = $"Claim {claimId} approved.";
            }
            return RedirectToAction("AcademicManagerView");
        }

        [HttpPost]
        public IActionResult RejectClaim(string claimId)
        {
            var claim = pendingClaims.FirstOrDefault(c => c.ClaimID == claimId);
            if (claim != null)
            {
                claim.Status = "Rejected";
                TempData["Message"] = $"Claim {claimId} rejected.";
            }
            return RedirectToAction("AcademicManagerView");
        }
    }
}
