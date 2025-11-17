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

        //Policy check method
        private bool MeetsPolicy(Claim c)
        {
            return c.HoursWorked > 0
                && c.HoursWorked <= 200   // prevent unrealistic hours
                && c.HourlyRate >= 100    // minimum rate
                && c.HourlyRate <= 1000   // maximum rate
                && c.TotalAmount == c.HoursWorked * c.HourlyRate; // consistency check
        }

        private void UpdateClaimStatus(string claimId, string newStatus)
        {
            var claim = pendingClaims.FirstOrDefault(c => c.ClaimID == claimId);
            if (claim != null)
            {
                // Run policy checks before verifying
                if (newStatus == "Verified" && !MeetsPolicy(claim))
                {
                    TempData["Message"] = $"Claim {claimId} failed policy checks.";
                    return;
                }

                claim.Status = newStatus;

                switch (newStatus)
                {
                    case "Verified": claim.VerifiedAt = DateTime.UtcNow; break;
                    case "Approved": claim.ApprovedAt = DateTime.UtcNow; break;
                    case "Rejected": claim.RejectedAt = DateTime.UtcNow; break;
                }

                TempData["Message"] = $"Claim {claimId} {newStatus.ToLower()}.";
            }
        }
    }
}
