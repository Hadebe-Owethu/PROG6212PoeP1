using Microsoft.AspNetCore.Mvc;
using ProgPOEP1.Models;

namespace ProgPOEP1.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Summary()
        {
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
