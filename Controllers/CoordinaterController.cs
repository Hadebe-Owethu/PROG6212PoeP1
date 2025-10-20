using Microsoft.AspNetCore.Mvc;
using ProgPOEP1.Models;

namespace ProgPOEP1.Controllers
{
    public class CoordinatorController : Controller
    {
        public static List<Claim> pendingClaims = new List<Claim>();

        public IActionResult ReviewClaims()
        {
            ViewBag.PendingClaims = pendingClaims;
            ViewBag.Message = TempData["Message"];
            ViewBag.Role = "Coordinator";
            return View("ReviewClaims");
        }

        [HttpPost]
        public IActionResult ApproveClaim(string claimId)
        {
            UpdateClaimStatus(claimId, "Approved");
            return RedirectToAction("ReviewClaims");
        }

        [HttpPost]
        public IActionResult RejectClaim(string claimId)
        {
            UpdateClaimStatus(claimId, "Rejected");
            return RedirectToAction("ReviewClaims");
        }

        [HttpPost]
        public IActionResult VerifyClaim(string claimId)
        {
            UpdateClaimStatus(claimId, "Verified");
            return RedirectToAction("ReviewClaims");
        }

        private void UpdateClaimStatus(string claimId, string newStatus)
        {
            var claim = pendingClaims.FirstOrDefault(c => c.ClaimID == claimId);
            if (claim != null)
            {
                claim.Status = newStatus;
                TempData["Message"] = $"Claim {claimId} {newStatus.ToLower()}.";
            }
        }
    }
}
