namespace ProgPOEP1.Models
{
    public class DashboardStats
    {
        public int TotalLecturers { get; set; }
        public int TotalClaims { get; set; }
        public int PendingVerification { get; set; }
        public int PendingApproval { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalVerified { get; set; }
        public int TotalApproved { get; set; }
        public decimal MonthlyTotal { get; set; }
        public decimal YearlyTotal { get; set; }

    }
}
