using ActualSite.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace ActualSite.Controllers
{
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DishesContext dbContext;
        public HomeController(ILogger<HomeController> logger, DishesContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dishes = await dbContext.Dishes.ToListAsync();
            return View(dishes);
        }

        [HttpGet]
        public IActionResult DishInfo(int id)
        {
            var dish = dbContext.Dishes.FirstOrDefault(d => d.IdDish == id);
            return View(dish);
        }

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Order()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Dish dish)
        {
            dbContext.Dishes.Add(dish);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }


        [Authorize(Policy = "AdminPolicy")]
        public IActionResult Edit(int id)
        {
            var dish = dbContext.Dishes.FirstOrDefault(d => d.IdDish == id);
            if (dish == null)
                return RedirectToAction("Index");
            return View(dish);
        }

        [HttpPost]
        public IActionResult Edit(Dish dish)
        {
            dbContext.Dishes.Update(dish);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpGet]
        [ActionName("Delete")]
        public IActionResult ConfirmDelete(int id)
        {
            var dish = dbContext.Dishes.FirstOrDefault(d => d.IdDish == id);
            if (dish == null)
                return RedirectToAction("Index");
            return View(dish);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var dish = await dbContext.Dishes.FirstOrDefaultAsync(d => d.IdDish == id);
            if(dish == null)
                return NotFound();
            dbContext.Dishes.Remove(dish);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
