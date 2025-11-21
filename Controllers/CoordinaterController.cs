using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using ProgPOEP1.Models;

namespace ProgPOEP1.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly IConfiguration _configuration;

        public CoordinatorController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ConnectionString => _configuration.GetConnectionString("DefaultConnection");

        private bool IsCoordinatorLoggedIn()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetString("UserId");

            Console.WriteLine($"Coordinator login check - UserRole: {userRole}, UserId: {userId}");

            return userRole == "Coordinator" && !string.IsNullOrEmpty(userId);
        }

        // ADD THIS MISSING DASHBOARD ACTION
        public IActionResult Dashboard()
        {
            if (!IsCoordinatorLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var pendingClaims = GetPendingClaims();
            return View(pendingClaims);
        }

        // REMOVE THIS DUPLICATE METHOD - You already have IsCoordinatorLoggedIn above
        // private bool IsCoordinatorLoggedIn() 
        // {
        //     return HttpContext.Session.GetString("UserRole") == "Coordinator";
        // }

        // REMOVE ProcessLogin - Your AccountController handles authentication directly
        // public IActionResult ProcessLogin(string username, string password)
        // {
        //     // This is not needed since AccountController handles the login
        // }

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
                        INNER JOIN Lecturers l ON c.ContractorID = l.LecturerID
                        WHERE c.Status = 'Pending' OR c.Status = 'Verified'
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
                                    ContractorID = reader["ContractorID"].ToString(),
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

        [HttpPost]
        public IActionResult VerifyClaim(string claimId)
        {
            if (!IsCoordinatorLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Update claim status
                    var query = "UPDATE Claims SET Status = 'Verified' WHERE ClaimID = @ClaimId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClaimId", claimId);
                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Record approval
                            var approvalQuery = @"
                                INSERT INTO ClaimApprovals (ClaimID, ApprovedBy, Action, Timestamp)
                                VALUES (@ClaimID, @ApprovedBy, @Action, @Timestamp)";

                            using (var approvalCommand = new SqlCommand(approvalQuery, connection))
                            {
                                approvalCommand.Parameters.AddWithValue("@ClaimID", claimId);
                                approvalCommand.Parameters.AddWithValue("@ApprovedBy", HttpContext.Session.GetString("UserName"));
                                approvalCommand.Parameters.AddWithValue("@Action", "Verified");
                                approvalCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                                approvalCommand.ExecuteNonQuery();
                            }

                            TempData["Message"] = "Claim verified successfully!";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Failed to verify claim.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error verifying claim: {ex.Message}";
            }

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public IActionResult RejectClaim(string claimId, string comments)
        {
            if (!IsCoordinatorLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Update claim status
                    var query = "UPDATE Claims SET Status = 'Rejected', Notes = @Notes WHERE ClaimID = @ClaimId";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClaimId", claimId);
                        command.Parameters.AddWithValue("@Notes", comments ?? "Rejected by coordinator");
                        var rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Record approval
                            var approvalQuery = @"
                                INSERT INTO ClaimApprovals (ClaimID, ApprovedBy, Action, Comments, Timestamp)
                                VALUES (@ClaimID, @ApprovedBy, @Action, @Comments, @Timestamp)";

                            using (var approvalCommand = new SqlCommand(approvalQuery, connection))
                            {
                                approvalCommand.Parameters.AddWithValue("@ClaimID", claimId);
                                approvalCommand.Parameters.AddWithValue("@ApprovedBy", HttpContext.Session.GetString("UserName"));
                                approvalCommand.Parameters.AddWithValue("@Action", "Rejected");
                                approvalCommand.Parameters.AddWithValue("@Comments", comments ?? "Rejected by coordinator");
                                approvalCommand.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                                approvalCommand.ExecuteNonQuery();
                            }

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

            return RedirectToAction("Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // ADD DEBUG ACTION FOR TESTING
        public IActionResult Debug()
        {
            var sessionInfo = new
            {
                UserId = HttpContext.Session.GetString("UserId"),
                UserName = HttpContext.Session.GetString("UserName"),
                UserRole = HttpContext.Session.GetString("UserRole"),
                IsCoordinatorLoggedIn = IsCoordinatorLoggedIn()
            };

            Console.WriteLine($"Coordinator Debug - UserId: {sessionInfo.UserId}, Role: {sessionInfo.UserRole}");

            return Json(sessionInfo);
        }
    }
}