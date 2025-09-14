using Microsoft.AspNetCore.Mvc;
using ProgPOEP1.Models;

namespace ProgPOEP1.Controllers
{
    public class CoordinatorController : Controller
    {
        public IActionResult ReviewClaims()
        {
            var pendingClaims = new List<Claim>
            {
                new Claim { ClaimID = "CLM001", ContractorID = "CTR001", Month = "September", HoursWorked = 35, Status = "Submitted" }
            };

            ViewBag.PendingClaims = pendingClaims;
            return View();
        }
    }
}
