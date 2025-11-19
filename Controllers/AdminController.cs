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

        public async Task<IActionResult> Summary()
        {
            var allClaims = await _context.Claims
                .Include(c => c.Contractor) // optional: if you want lecturer info
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Claims = allClaims;
            return View();
        }
    }
}
