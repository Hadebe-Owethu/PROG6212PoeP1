using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ProgPOEP1.Data;
using System.Linq;

namespace ProgPOEP1.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Message = TempData["Message"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password, string role)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(role))
            {
                TempData["Message"] = "All fields are required.";
                return RedirectToAction("Login");
            }

            if (role == "Lecturer")
            {
                var lecturer = _context.Lecturers
                    .FirstOrDefault(l => l.Username == username && l.Password == password && l.IsApproved);

                if (lecturer != null)
                {
                    HttpContext.Session.SetString("LecturerID", lecturer.LecturerID);
                    HttpContext.Session.SetString("FullName", lecturer.FullName);
                    HttpContext.Session.SetString("Role", "Lecturer");

                    return RedirectToAction("Dashboard", "Lecturer");
                }

                TempData["Message"] = "Invalid lecturer credentials or not approved.";
                return RedirectToAction("Login");
            }

            // ✅ Coordinator login (hardcoded)
            if (role == "Coordinator" && username == "coord" && password == "coord123")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminUser", username);
                HttpContext.Session.SetString("Role", "Coordinator");

                return RedirectToAction("AdminDashboard", "ProgrammeCoordinator");
            }

            // ✅ Academic Manager login (hardcoded)
            if (role == "AcademicManager" && username == "manager" && password == "manager123")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminUser", username);
                HttpContext.Session.SetString("Role", "AcademicManager");

                return RedirectToAction("AdminDashboard", "AcademicManager");
            }

            // ✅ HR login (optional hardcoded)
            if (role == "HR" && username == "hradmin" && password == "hr123")
            {
                HttpContext.Session.SetString("IsAdmin", "true");
                HttpContext.Session.SetString("AdminUser", username);
                HttpContext.Session.SetString("Role", "HR");

                return RedirectToAction("Dashboard", "HR");
            }

            TempData["Message"] = "Invalid credentials or role.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
