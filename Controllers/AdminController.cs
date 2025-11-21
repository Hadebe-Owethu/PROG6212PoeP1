using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using ProgPOEP1.Models;
using System;
using System.Collections.Generic;

namespace ProgPOEP1.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ConnectionString => _configuration.GetConnectionString("DefaultConnection");

        private bool IsAdminLoggedIn()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }

        public IActionResult Dashboard()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var stats = GetDashboardStats();
            var verifiedClaims = GetVerifiedClaims();
            var approvedClaims = GetApprovedClaims();

            ViewBag.Stats = stats;
            ViewBag.VerifiedClaims = verifiedClaims;
            ViewBag.ApprovedClaims = approvedClaims;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View();
        }

        [HttpPost]
        public JsonResult ApproveClaim(string claimId)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    var query = "UPDATE Claims SET Status = 'Approved', ApprovedAt = GETDATE() WHERE ClaimID = @ClaimID";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClaimID", claimId);
                        int rows = command.ExecuteNonQuery();
                        return Json(new { success = rows > 0, message = rows > 0 ? "Claim approved." : "Claim not found." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult RejectClaim(string claimId, string reason)
        {
            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    var query = "UPDATE Claims SET Status = 'Rejected', Notes = @Reason WHERE ClaimID = @ClaimID";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClaimID", claimId);
                        command.Parameters.AddWithValue("@Reason", reason);
                        int rows = command.ExecuteNonQuery();
                        return Json(new { success = rows > 0, message = rows > 0 ? "Claim rejected." : "Claim not found." });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private DashboardStats GetDashboardStats()
        {
            var stats = new DashboardStats();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    var query1 = "SELECT COUNT(*) FROM Lecturers";
                    using (var command = new SqlCommand(query1, connection))
                        stats.TotalLecturers = (int)command.ExecuteScalar();

                    var query2 = "SELECT COUNT(*) FROM Claims";
                    using (var command = new SqlCommand(query2, connection))
                        stats.TotalClaims = (int)command.ExecuteScalar();

                    var query3 = "SELECT COUNT(*) FROM Claims WHERE Status = 'Pending'";
                    using (var command = new SqlCommand(query3, connection))
                        stats.PendingVerification = (int)command.ExecuteScalar();

                    var query4 = "SELECT COUNT(*) FROM Claims WHERE Status = 'Verified'";
                    using (var command = new SqlCommand(query4, connection))
                        stats.PendingApproval = (int)command.ExecuteScalar();

                    var query6 = "SELECT COUNT(*) FROM Claims WHERE Status = 'Verified'";
                    using (var command = new SqlCommand(query6, connection))
                        stats.TotalVerified = (int)command.ExecuteScalar();

                    var query7 = "SELECT COUNT(*) FROM Claims WHERE Status = 'Approved'";
                    using (var command = new SqlCommand(query7, connection))
                        stats.TotalApproved = (int)command.ExecuteScalar();

                    var query8 = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Claims WHERE Status = 'Approved' AND MONTH(CreatedAt) = MONTH(GETDATE())";
                    using (var command = new SqlCommand(query8, connection))
                        stats.MonthlyTotal = Convert.ToDecimal(command.ExecuteScalar());

                    var query9 = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Claims WHERE Status = 'Approved' AND YEAR(CreatedAt) = YEAR(GETDATE())";
                    using (var command = new SqlCommand(query9, connection))
                        stats.YearlyTotal = Convert.ToDecimal(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting admin stats: {ex.Message}");
            }

            return stats;
        }

        private List<Claim> GetVerifiedClaims()
        {
            var claims = new List<Claim>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var query = @"
            SELECT c.ClaimID, c.ContractorID, l.FullName, c.Month, c.HoursWorked, c.HourlyRate,
                   c.TotalAmount, c.Status, c.CreatedAt, c.Notes,
                   c.VerifiedAt, c.VerifiedBy
            FROM Claims c
            INNER JOIN Lecturers l ON c.ContractorID = l.LecturerID
            WHERE c.Status = 'Verified'
            ORDER BY c.CreatedAt DESC";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var claim = new Claim
                        {
                            ClaimID = reader["ClaimID"].ToString(),
                            ContractorID = reader["ContractorID"].ToString(),
                            Month = reader["Month"].ToString(),
                            HoursWorked = Convert.ToDecimal(reader["HoursWorked"]),
                            HourlyRate = Convert.ToDecimal(reader["HourlyRate"]),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                            Status = reader["Status"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            Notes = reader["Notes"]?.ToString() ?? ""
                        };

                        // 👉 Add these lines here
                        claim.VerifiedAt = reader["VerifiedAt"] != DBNull.Value
                            ? Convert.ToDateTime(reader["VerifiedAt"])
                            : (DateTime?)null;

                        claim.VerifiedBy = reader["VerifiedBy"]?.ToString();

                        // Optional: populate Lecturer navigation property
                        claim.Lecturer = new Lecturer
                        {
                            LecturerID = reader["ContractorID"].ToString(),
                            FullName = reader["FullName"].ToString()
                        };

                        claims.Add(claim);
                    }
                }
            }

            return claims;
        }


        public List<Claim> GetApprovedClaims()
        {
            var claims = new List<Claim>();

            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                var query = @"
            SELECT c.ClaimID, c.ContractorID, l.FullName, c.Month, c.HoursWorked, c.HourlyRate,
                   c.TotalAmount, c.Status, c.CreatedAt, c.Notes,
                   c.ApprovedAt
            FROM Claims c
            INNER JOIN Lecturers l ON c.ContractorID = l.LecturerID
            WHERE c.Status = 'Approved'
            ORDER BY c.CreatedAt DESC";

                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var claim = new Claim
                        {
                            ClaimID = reader["ClaimID"].ToString(),
                            ContractorID = reader["ContractorID"].ToString(),
                            Month = reader["Month"].ToString(),
                            HoursWorked = Convert.ToDecimal(reader["HoursWorked"]),
                            HourlyRate = Convert.ToDecimal(reader["HourlyRate"]),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                            Status = reader["Status"].ToString(),
                            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                            Notes = reader["Notes"]?.ToString() ?? ""
                        };

                        // 👉 Add this line here
                        claim.ApprovedAt = reader["ApprovedAt"] != DBNull.Value
                            ? Convert.ToDateTime(reader["ApprovedAt"])
                            : (DateTime?)null;

                        // Optional: populate Lecturer navigation property
                        claim.Lecturer = new Lecturer
                        {
                            LecturerID = reader["ContractorID"].ToString(),
                            FullName = reader["FullName"].ToString()
                        };

                        claims.Add(claim);
                    }
                }
            }

            return claims;
        }

    }


}
