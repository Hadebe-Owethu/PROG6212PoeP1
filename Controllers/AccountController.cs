using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using ProgPOEP1.Models;

namespace ProgPOEP1.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ConnectionString => _configuration.GetConnectionString("DefaultConnection");

        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost]
        public IActionResult Login(string role, string username, string password)
        {
            Console.WriteLine($"Login attempt: Role={role}, Username={username}");

            if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Please fill in all fields.";
                return View();
            }

            try
            {
                switch (role)
                {
                    case "Lecturer":
                        Console.WriteLine("Attempting lecturer authentication...");
                        var lecturer = AuthenticateLecturer(username, password);
                        if (lecturer != null)
                        {
                            Console.WriteLine($"Lecturer found: {lecturer.FullName}, IsApproved: {lecturer.IsApproved}");

                            if (lecturer.IsApproved)
                            {
                                SetupLecturerSession(lecturer);
                                TempData["LoginMessage"] = $"Welcome back, {lecturer.FullName}!";
                                Console.WriteLine("Redirecting to Lecturer Dashboard...");
                                return RedirectToAction("Dashboard", "Lecturer");
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Your account is pending approval. Please contact HR.";
                                Console.WriteLine("Lecturer account not approved.");
                            }
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Invalid lecturer credentials.";
                            Console.WriteLine("Lecturer authentication failed.");
                        }
                        break;

                    case "Coordinator":
                        if (username == "coordinator" && password == "coord123")
                        {
                            SetupCoordinatorSession(username);
                            return RedirectToAction("Dashboard", "Coordinator");
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Invalid coordinator credentials.";
                        }
                        break;

                    case "HR":
                        if (username == "hr" && password == "hr123")
                        {
                            SetupHRSession(username);
                            return RedirectToAction("Dashboard", "HR");
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Invalid HR credentials.";
                        }
                        break;

                    case "Admin":
                        if (username == "admin" && password == "admin123")
                        {
                            SetupAdminSession(username);
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Invalid admin credentials.";
                        }
                        break;

                    default:
                        ViewBag.ErrorMessage = "Please select a valid role.";
                        return View();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                ViewBag.ErrorMessage = "An error occurred during login. Please try again.";
            }

            return View();
        }

        private Lecturer AuthenticateLecturer(string username, string password)
        {
            // TEMPORARY: Always return a test lecturer to see if login works
            Console.WriteLine("=== TEMPORARY LOGIN BYPASS ===");
            if (username == "owethu" && password == "pass123")
            {
                return new Lecturer
                {
                    LecturerID = "LECT001",
                    FullName = "Owethu Mkhize",
                    Email = "owethu@yahoo.com",
                    Department = "Computer Science",
                    HourlyRate = 250.00m,
                    Username = "owethu",
                    IsApproved = true
                };
            }

            // Also test with any credentials
            Console.WriteLine($"Attempting login with: {username}");
            return new Lecturer
            {
                LecturerID = "TEST001",
                FullName = "Test Lecturer",
                Email = "test@email.com",
                Department = "Computer Science",
                HourlyRate = 250.00m,
                Username = username,
                IsApproved = true
            };
        }

        private void SetupLecturerSession(Lecturer lecturer)
        {
            HttpContext.Session.SetString("UserId", lecturer.LecturerID);
            HttpContext.Session.SetString("UserName", lecturer.FullName);
            HttpContext.Session.SetString("UserEmail", lecturer.Email);
            HttpContext.Session.SetString("UserDepartment", lecturer.Department);
            HttpContext.Session.SetString("HourlyRate", lecturer.HourlyRate.ToString("F2"));
            HttpContext.Session.SetString("UserRole", "Lecturer");
            Console.WriteLine("Lecturer session setup complete.");
        }

        private void SetupCoordinatorSession(string username)
        {
            HttpContext.Session.SetString("UserId", username);
            HttpContext.Session.SetString("UserName", "Programme Coordinator");
            HttpContext.Session.SetString("UserRole", "Coordinator");
        }

        private void SetupHRSession(string username)
        {
            HttpContext.Session.SetString("UserId", username);
            HttpContext.Session.SetString("UserName", "HR Manager");
            HttpContext.Session.SetString("UserRole", "HR");
        }

        private void SetupAdminSession(string username)
        {
            HttpContext.Session.SetString("UserId", username);
            HttpContext.Session.SetString("UserName", "Academic Manager");
            HttpContext.Session.SetString("UserRole", "Admin");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["LogoutMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }
    }
}