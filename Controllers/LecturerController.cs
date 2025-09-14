using Microsoft.AspNetCore.Mvc;
using ProgPOEP1.Models;
namespace ProgPOEP1.Controllers
{
        public class LecturerController : Controller
        {
            public IActionResult Dashboard()
            {
                // Simulated static data for prototype
                var claims = new List<Claim>
            {
                new Claim { Month = "August", HoursWorked = 40, Status = "Approved" },
                new Claim { Month = "September", HoursWorked = 35, Status = "Pending" }
            };

                ViewBag.Claims = claims;
                return View();
            }

            public IActionResult SubmitClaim()
            {
                return View();
            }
        }
    }

