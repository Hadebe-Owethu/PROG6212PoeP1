using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using ProgPOEP1.Models;

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
            var recentClaims = GetRecentClaims();

            ViewBag.Stats = stats;
            ViewBag.RecentClaims = recentClaims;
            ViewBag.AdminName = HttpContext.Session.GetString("UserName");

            return View();
        }

        public IActionResult SystemOverview()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var systemData = GetSystemOverviewData();
            return View(systemData);
        }

        public IActionResult ApprovalWorkflow()
        {
            if (!IsAdminLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var workflowData = GetApprovalWorkflowData();
            return View(workflowData);
        }

        private DashboardStats GetDashboardStats()
        {
            var stats = new DashboardStats();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Total lecturers
                    var query1 = "SELECT COUNT(*) FROM Lecturers";
                    using (var command = new SqlCommand(query1, connection))
                    {
                        stats.TotalLecturers = (int)command.ExecuteScalar();
                    }

                    // Total claims
                    var query2 = "SELECT COUNT(*) FROM Claims";
                    using (var command = new SqlCommand(query2, connection))
                    {
                        stats.TotalClaims = (int)command.ExecuteScalar();
                    }

                    // Claims pending coordinator verification
                    var query3 = "SELECT COUNT(*) FROM Claims WHERE Status = 'Pending'";
                    using (var command = new SqlCommand(query3, connection))
                    {
                        stats.PendingVerification = (int)command.ExecuteScalar();
                    }

                    // Claims pending HR approval
                    var query4 = "SELECT COUNT(*) FROM Claims WHERE Status = 'Verified'";
                    using (var command = new SqlCommand(query4, connection))
                    {
                        stats.PendingApproval = (int)command.ExecuteScalar();
                    }

                    // Total amount paid this month
                    var query5 = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Claims WHERE Status = 'Approved' AND MONTH(CreatedAt) = MONTH(GETDATE())";
                    using (var command = new SqlCommand(query5, connection))
                    {
                        stats.TotalAmount = Convert.ToDecimal(command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting admin stats: {ex.Message}");
            }

            return stats;
        }

        private List<Claim> GetRecentClaims()
        {
            var claims = new List<Claim>();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    var query = @"
                        SELECT c.ClaimID, c.LecturerID, l.FullName, c.Month, c.HoursWorked, c.HourlyRate, 
                               c.TotalAmount, c.Status, c.CreatedAt, c.Notes
                        FROM Claims c
                        INNER JOIN Lecturers l ON c.LecturerID = l.LecturerID
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
                                    ContractorID = reader["LecturerID"].ToString(),
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
                Console.WriteLine($"Error getting recent claims: {ex.Message}");
            }

            return claims.Take(10).ToList(); // Return only 10 most recent
        }

        private SystemOverview GetSystemOverviewData()
        {
            var overview = new SystemOverview();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Get department statistics
                    var deptQuery = @"
                        SELECT Department, COUNT(*) as LecturerCount, 
                               AVG(HourlyRate) as AvgHourlyRate
                        FROM Lecturers 
                        GROUP BY Department";

                    using (var command = new SqlCommand(deptQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            overview.DepartmentStats = new List<DepartmentStat>();
                            while (reader.Read())
                            {
                                overview.DepartmentStats.Add(new DepartmentStat
                                {
                                    Department = reader["Department"].ToString(),
                                    LecturerCount = Convert.ToInt32(reader["LecturerCount"]),
                                    AverageHourlyRate = Convert.ToDecimal(reader["AvgHourlyRate"])
                                });
                            }
                        }
                    }

                    // Get claim status distribution
                    var statusQuery = @"
                        SELECT Status, COUNT(*) as Count
                        FROM Claims 
                        GROUP BY Status";

                    using (var command = new SqlCommand(statusQuery, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            overview.ClaimStatusDistribution = new Dictionary<string, int>();
                            while (reader.Read())
                            {
                                overview.ClaimStatusDistribution.Add(
                                    reader["Status"].ToString(),
                                    Convert.ToInt32(reader["Count"])
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting system overview: {ex.Message}");
            }

            return overview;
        }

        private ApprovalWorkflow GetApprovalWorkflowData()
        {
            var workflow = new ApprovalWorkflow();

            try
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();

                    // Get claims in workflow
                    var query = @"
                        SELECT c.ClaimID, l.FullName, c.Month, c.TotalAmount, c.Status, c.CreatedAt,
                               ca.ApprovedBy, ca.Action, ca.Timestamp
                        FROM Claims c
                        INNER JOIN Lecturers l ON c.LecturerID = l.LecturerID
                        LEFT JOIN ClaimApprovals ca ON c.ClaimID = ca.ClaimID
                        WHERE c.Status IN ('Pending', 'Verified')
                        ORDER BY c.CreatedAt DESC";

                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            workflow.ClaimsInWorkflow = new List<WorkflowClaim>();
                            while (reader.Read())
                            {
                                workflow.ClaimsInWorkflow.Add(new WorkflowClaim
                                {
                                    ClaimID = reader["ClaimID"].ToString(),
                                    LecturerName = reader["FullName"].ToString(),
                                    Month = reader["Month"].ToString(),
                                    TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                                    Status = reader["Status"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                    LastActionBy = reader["ApprovedBy"]?.ToString(),
                                    LastAction = reader["Action"]?.ToString(),
                                    LastActionTime = reader["Timestamp"] != DBNull.Value ? Convert.ToDateTime(reader["Timestamp"]) : (DateTime?)null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting workflow data: {ex.Message}");
            }

            return workflow;
        }
    }

    public class DashboardStats
    {
        public int TotalLecturers { get; set; }
        public int TotalClaims { get; set; }
        public int PendingVerification { get; set; }
        public int PendingApproval { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class SystemOverview
    {
        public List<DepartmentStat> DepartmentStats { get; set; }
        public Dictionary<string, int> ClaimStatusDistribution { get; set; }
    }

    public class DepartmentStat
    {
        public string Department { get; set; }
        public int LecturerCount { get; set; }
        public decimal AverageHourlyRate { get; set; }
    }

    public class ApprovalWorkflow
    {
        public List<WorkflowClaim> ClaimsInWorkflow { get; set; }
    }

    public class WorkflowClaim
    {
        public string ClaimID { get; set; }
        public string LecturerName { get; set; }
        public string Month { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LastActionBy { get; set; }
        public string LastAction { get; set; }
        public DateTime? LastActionTime { get; set; }
    }
}