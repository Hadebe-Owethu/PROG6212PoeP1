using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgPOEP1.Data;

namespace ProgPOEP1.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> AdminDashboard(string? statusFilter = null)
        {
            var claims = await _context.Claims
                .Include(c => c.Contractor)
                .Where(c => string.IsNullOrEmpty(statusFilter) || c.Status == statusFilter)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Claims = claims;
            ViewBag.StatusFilter = statusFilter;
            ViewBag.AdminUser = HttpContext.Session.GetString("AdminUser");
            return View();
        }
    }
}
