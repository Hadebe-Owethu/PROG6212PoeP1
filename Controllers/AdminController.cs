using Microsoft.AspNetCore.Mvc;
using ProgPOEP1.Models;

namespace ProgPOEP1.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Summary()
        {
            var allClaims = CoordinatorController.pendingClaims;

            var summaryStats = new
            {
                TotalClaims = allClaims.Count,
                Pending = allClaims.Count(c => c.Status == "Pending"),
                Approved = allClaims.Count(c => c.Status == "Approved"),
                Rejected = allClaims.Count(c => c.Status == "Rejected")
            };

            ViewBag.Summary = summaryStats;
            return View();
        }
    }
}
