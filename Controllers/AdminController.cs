using Microsoft.AspNetCore.Mvc;
using ProgPOEP1.Models;

namespace ProgPOEP1.Controllers
{
    public class AdminController : Controller
    {
        // Displays summary statistics for all claims
        public IActionResult Summary()
        {
            // Simulated summary data for dashboard cards
            var summaryStats = new
            {
                TotalClaims = 25,
                Pending = 5,
                Approved = 18,
                Rejected = 2
            };

            ViewBag.Summary = summaryStats;
            return View();
        }
    }
}
