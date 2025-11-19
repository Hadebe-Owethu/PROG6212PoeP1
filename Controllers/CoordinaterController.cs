using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgPOEP1.Data;
using ProgPOEP1.Security;

namespace ProgPOEP1.Controllers
{
    [AdminAuthorize("Coordinator")]
    public class CoordinatorController : Controller
    {
        private readonly AppDbContext _context;

        public CoordinatorController(AppDbContext context)
        {
            _context = context;
        }

        // Admin dashboard with optional status filter and lecturer info
        public async Task<IActionResult> AdminDashboard(string? statusFilter = null)
        {
            var query = _context.Claims
                .Include(c => c.Contractor) // fetch lecturer details
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusFilter))
                query = query.Where(c => c.Status == statusFilter);

            var claims = await query
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Claims = claims;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.AdminUser = HttpContext.Session.GetString("AdminUser");
            ViewBag.Message = TempData["Message"];
            return View();
        }

        // Update status and stamp timestamps
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateClaimStatus(string claimId, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(claimId) || string.IsNullOrWhiteSpace(newStatus))
                return RedirectToAction("AdminDashboard");

            var claim = await _context.Claims.FirstOrDefaultAsync(c => c.ClaimID == claimId);
            if (claim == null)
                return RedirectToAction("AdminDashboard");

            claim.Status = newStatus;

            if (newStatus == "Verified")
            {
                claim.VerifiedAt = DateTime.UtcNow;
                claim.ApprovedAt = null;
                claim.RejectedAt = null;
            }
            else if (newStatus == "Approved")
            {
                claim.ApprovedAt = DateTime.UtcNow;
                claim.RejectedAt = null;
            }
            else if (newStatus == "Rejected")
            {
                claim.RejectedAt = DateTime.UtcNow;
                claim.ApprovedAt = null;
            }
            else if (newStatus == "Pending")
            {
                claim.VerifiedAt = null;
                claim.ApprovedAt = null;
                claim.RejectedAt = null;
            }

            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Claim {claimId} set to {newStatus}.";
            return RedirectToAction("AdminDashboard");
        }
    }
}
