using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using ProgPOEP1.Models;

namespace ProgPOEP1.Controllers
{
    public class HRController : Controller
    {
        private readonly IConfiguration _configuration;

        public HRController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ConnectionString => _configuration.GetConnectionString("DefaultConnection");

        private bool IsHRLoggedIn()
        {
            return HttpContext.Session.GetString("UserRole") == "HR";
        }

        public IActionResult LecturerManagement()
        {
            if (!IsHRLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var lecturers = GetAllLecturers();
            return View(lecturers);
        }

        public IActionResult CreateLecturer()
        {
            if (!IsHRLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateLecturer(Lecturer lecturer)
        {
            if (!IsHRLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    using (var connection = new SqlConnection(ConnectionString))
                    {
                        connection.Open();
                        var query = @"
                            INSERT INTO Lecturers (LecturerID, FullName, Email, Department, HourlyRate, Username, Password, IsApproved, CreatedAt)
                            VALUES (@LecturerID, @FullName, @Email, @Department, @HourlyRate, @Username, @Password, @IsApproved, @CreatedAt)";

                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@LecturerID", lecturer.LecturerID);
                            command.Parameters.AddWithValue("@FullName", lecturer.FullName);
                            command.Parameters.AddWithValue("@Email", lecturer.Email);
                            command.Parameters.AddWithValue("@Department", lecturer.Department);
                            command.Parameters.AddWithValue("@HourlyRate", lecturer.HourlyRate);
                            command.Parameters.AddWithValue("@Username", lecturer.Username);
                            command.Parameters.AddWithValue("@Password", lecturer.Password);
                            command.Parameters.AddWithValue("@IsApproved", lecturer.IsApproved);
                            command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                            var rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                TempData["Message"] = "Lecturer created successfully!";
                                return RedirectToAction("LecturerManagement");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating lecturer: {ex.Message}");
                }
            }
            return View(lecturer);
        }

        public IActionResult ClaimApprovals()
        {
            if (!IsHRLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var pendingClaims = GetPendingClaims();
            return View(pendingClaims);
        }

        [HttpPost]
        public IActionResult ApproveClaim(string claimId)
        {
            if (!IsHRLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Update claim status to Approved
                    var query = "UPDATE Claims SET Status = 'Approved' WHERE ClaimID = @ClaimId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClaimId", claimId);
                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            TempData["Message"] = "Claim approved successfully!";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to approve claim.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error approving claim: {ex.Message}";
            }

            return RedirectToAction("ClaimApprovals");
        }

        [HttpPost]
        public IActionResult RejectClaim(string claimId, string comments)
        {
            if (!IsHRLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Update claim status to Rejected
                    var query = "UPDATE Claims SET Status = 'Rejected', Notes = @Notes WHERE ClaimID = @ClaimId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClaimId", claimId);
                        command.Parameters.AddWithValue("@Notes", comments ?? "Rejected by HR");
                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            TempData["Message"] = "Claim rejected successfully!";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to reject claim.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error rejecting claim: {ex.Message}";
            }

            return RedirectToAction("ClaimApprovals");
        }

        private List<Lecturer> GetAllLecturers()
        {
            var lecturers = new List<Lecturer>();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    var query = @"
                        SELECT LecturerID, FullName, Email, Department, HourlyRate, Username, IsApproved, CreatedAt
                        FROM Lecturers 
                        ORDER BY CreatedAt DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                lecturers.Add(new Lecturer
                                {
                                    LecturerID = reader["LecturerID"].ToString(),
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Department = reader["Department"].ToString(),
                                    HourlyRate = Convert.ToDecimal(reader["HourlyRate"]),
                                    Username = reader["Username"].ToString(),
                                    IsApproved = Convert.ToBoolean(reader["IsApproved"]),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting lecturers: {ex.Message}");
            }

            return lecturers;
        }

        private List<Claim> GetPendingClaims()
        {
            var claims = new List<Claim>();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    var query = @"
                        SELECT c.ClaimID, c.ContractorID, l.FullName, c.Month, c.HoursWorked, c.HourlyRate, 
                               c.TotalAmount, c.Status, c.CreatedAt, c.Notes
                        FROM Claims c
                        INNER JOIN Lecturers l ON c.ContractorID = l.LecturerID  -- Changed to ContractorID
                        WHERE c.Status = 'Verified' OR c.Status = 'Pending'
                        ORDER BY c.CreatedAt DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                claims.Add(new Claim
                                {
                                    ClaimID = reader["ClaimID"].ToString(),
                                    ContractorID = reader["ContractorID"].ToString(), // Changed from LecturerID to ContractorID
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
                Console.WriteLine($"Error getting pending claims: {ex.Message}");
            }

            return claims;
        }
    }
}