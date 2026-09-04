using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Project.Data;
using Recipe_Project.Models;

namespace Recipe_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(AppDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var featuredRecipes = await _context.Recipes
                .Include(r => r.User)
                .Include(r => r.Reviews)
                .OrderByDescending(r => r.CreatedAt)
                .Take(8)
                .ToListAsync();

            ViewBag.Categories = await _context.Recipes
                .Select(r => r.Category)
                .Distinct()
                .ToListAsync();

            return View(featuredRecipes);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Signup()
        {
            return RedirectToAction("Register", "Account");
        }

        public IActionResult Login()
        {
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> about()
        {
            ViewBag.TotalRecipes = await _context.Recipes.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalReviews = await _context.Reviews.CountAsync();
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
