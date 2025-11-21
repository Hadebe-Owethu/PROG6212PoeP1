using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using ProgPOEP1.Models;
using ProgPOEP1.ViewModels;

namespace ProgPOEP1.Controllers
{
    public class LecturerController : Controller
    {
        private readonly IConfiguration _configuration;

        public LecturerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ConnectionString => _configuration.GetConnectionString("DefaultConnection");

        private bool IsLecturerLoggedIn()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");
            Console.WriteLine($"IsLecturerLoggedIn check - UserRole: {userRole}, UserId: {userId}");
            return userRole == "Lecturer" && !string.IsNullOrEmpty(userId);
        }

        public IActionResult Dashboard()
        {
            Console.WriteLine("=== DASHBOARD ACCESSED ===");

            if (!IsLecturerLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login to access the dashboard.";
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetString("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            var hourlyRate = HttpContext.Session.GetString("HourlyRate");
            var department = HttpContext.Session.GetString("UserDepartment");

            ViewBag.LecturerName = userName;
            ViewBag.HourlyRate = hourlyRate;
            ViewBag.Department = department;

            var claims = GetLecturerClaims(userId);
            ViewBag.Claims = claims;
            ViewBag.TotalClaims = claims.Count;
            ViewBag.ApprovedCount = claims.Count(c => c.Status == "Approved");
            ViewBag.PendingCount = claims.Count(c => c.Status == "Pending" || c.Status == "Verified");

            if (TempData["LoginMessage"] != null)
                ViewBag.Message = TempData["LoginMessage"];
            else if (TempData["Message"] != null)
                ViewBag.Message = TempData["Message"];

            return View();
        }

        public IActionResult SubmitClaim()
        {
            Console.WriteLine("=== SUBMIT CLAIM GET ===");

            if (!IsLecturerLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login to submit a claim.";
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetString("UserId");
            var userName = HttpContext.Session.GetString("UserName");
            var hourlyRate = HttpContext.Session.GetString("HourlyRate");
            var department = HttpContext.Session.GetString("UserDepartment");

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "User session not found. Please login again.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(hourlyRate))
            {
                TempData["ErrorMessage"] = "Hourly rate not found in session. Please contact HR.";
                return RedirectToAction("Dashboard");
            }

            ViewBag.LecturerName = userName;
            ViewBag.HourlyRate = decimal.Parse(hourlyRate);
            ViewBag.Department = department;

            var vm = new ClaimSubmissionViewModel(); 
            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitClaim(ClaimSubmissionViewModel vm)
        {
            if (!IsLecturerLoggedIn())
            {
                TempData["ErrorMessage"] = "Please login to submit a claim.";
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("UserId");
                var hourlyRateStr = HttpContext.Session.GetString("HourlyRate");

                var claim = new Claim
                {
                    ClaimID = "CLM-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    ContractorID = userId,
                    HourlyRate = decimal.Parse(hourlyRateStr),
                    Month = vm.Month,
                    HoursWorked = vm.HoursWorked,
                    Notes = vm.Notes,
                    Status = "Pending",
                    CreatedAt = DateTime.Now,
                    Lecturer = null 
                };

                if (vm.DocumentFile != null && vm.DocumentFile.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    Directory.CreateDirectory(uploads);
                    var safeFileName = Path.GetFileName(vm.DocumentFile.FileName);
                    var filePath = Path.Combine(uploads, safeFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        vm.DocumentFile.CopyTo(stream);
                    }

                    claim.DocumentPath = "/uploads/" + safeFileName;
                }
                else
                {
                    claim.DocumentPath = ""; 
                }

                var result = SaveClaimToDatabase(claim);

                if (result)
                {
                    TempData["Message"] = "Claim submitted successfully!";
                    return RedirectToAction("Dashboard");
                }

                ModelState.AddModelError("", "Error saving claim. Please try again.");
            }

            return View(vm);
        }


        private bool SaveClaimToDatabase(Claim claim)
        {
            try
            {
                Console.WriteLine("=== SAVE CLAIM TO DATABASE ===");

                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    Console.WriteLine("Database connection opened successfully");

                    if (string.IsNullOrEmpty(claim.ClaimID))
                        claim.ClaimID = "CLM-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                    var query = @"
                        INSERT INTO Claims (
                            ClaimID, 
                            ContractorID, 
                            Month, 
                            HoursWorked, 
                            HourlyRate, 
                            DocumentPath, 
                            Status, 
                            CreatedAt, 
                            Notes
                        ) VALUES (
                            @ClaimID, 
                            @ContractorID, 
                            @Month, 
                            @HoursWorked, 
                            @HourlyRate, 
                            @DocumentPath, 
                            @Status, 
                            @CreatedAt, 
                            @Notes
                        )";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClaimID", claim.ClaimID);
                        command.Parameters.AddWithValue("@ContractorID", claim.ContractorID);
                        command.Parameters.AddWithValue("@Month", claim.Month);
                        command.Parameters.AddWithValue("@HoursWorked", claim.HoursWorked);
                        command.Parameters.AddWithValue("@HourlyRate", claim.HourlyRate);
                        command.Parameters.AddWithValue("@DocumentPath", claim.DocumentPath ?? "");
                        command.Parameters.AddWithValue("@Status", claim.Status ?? "Pending");
                        command.Parameters.AddWithValue("@CreatedAt", claim.CreatedAt == default ? DateTime.Now : claim.CreatedAt);
                        command.Parameters.AddWithValue("@Notes", claim.Notes ?? "");

                        var rowsAffected = command.ExecuteNonQuery();
                        Console.WriteLine($"Rows affected: {rowsAffected}");
                        return rowsAffected > 0;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine($"❌ SQL Error saving claim: {sqlEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ General Error saving claim: {ex.Message}");
                return false;
            }
        }

        private List<Claim> GetLecturerClaims(string contractorId)
        {
            var claims = new List<Claim>();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    var query = @"
                        SELECT ClaimID, Month, HoursWorked, HourlyRate, TotalAmount, Status, CreatedAt, Notes
                        FROM Claims 
                        WHERE ContractorID = @ContractorID
                        ORDER BY CreatedAt DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ContractorID", contractorId);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                claims.Add(new Claim
                                {
                                    ClaimID = reader["ClaimID"].ToString(),
                                    Month = reader["Month"].ToString(),
                                    HoursWorked = Convert.ToDecimal(reader["HoursWorked"]),
                                    HourlyRate = Convert.ToDecimal(reader["HourlyRate"]),
                                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                    Status = reader["Status"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                    Notes = reader["Notes"]?.ToString() ?? ""
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting claims: {ex.Message}");
            }

            return claims;
        }

        public IActionResult MyClaims()
        {
            if (!IsLecturerLoggedIn())
                return RedirectToAction("Login", "Account");

            var lecturerId = HttpContext.Session.GetString("UserId");
            var claims = GetLecturerClaims(lecturerId);
            return View(claims);
        }

        public IActionResult DebugSession()
        {
            var sessionData = new
            {
                UserId = HttpContext.Session.GetString("UserId"),
                UserName = HttpContext.Session.GetString("UserName"),
                UserRole = HttpContext.Session.GetString("UserRole"),
                HourlyRate = HttpContext.Session.GetString("HourlyRate"),
                UserDepartment = HttpContext.Session.GetString("UserDepartment"),
                IsLecturerLoggedIn = IsLecturerLoggedIn(),
                SessionId = HttpContext.Session.Id
            };

            Console.WriteLine("=== SESSION DEBUG ===");
            Console.WriteLine($"UserId: {sessionData.UserId}");
            Console.WriteLine($"UserName: {sessionData.UserName}");
            Console.WriteLine($"UserRole: {sessionData.UserRole}");
            Console.WriteLine($"HourlyRate: {sessionData.HourlyRate}");
            Console.WriteLine($"UserDepartment: {sessionData.UserDepartment}");
            Console.WriteLine($"IsLecturerLoggedIn: {sessionData.IsLecturerLoggedIn}");
            Console.WriteLine($"SessionId: {sessionData.SessionId}");

            return Json(sessionData);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Message"] = "You have been logged out successfully.";
            return RedirectToAction("Login", "Account");
        }
    }
}
