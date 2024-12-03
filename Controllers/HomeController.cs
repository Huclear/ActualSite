using ActualSite.data;
using ActualSite.Models;
using Humanizer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
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
        public IActionResult Cart()
        {
            List<CartDish> cart;

            try
            {
                if (HttpContext.Session.Keys.Contains(SiteInfo.CART_ITEMS_KEY)
                && HttpContext.Session.GetObject<IEnumerable<CartDish>>(SiteInfo.CART_ITEMS_KEY) is List<CartDish> cartItems) 
                    cart = cartItems;
                else cart = new List<CartDish>();
            }
            catch (Exception ex)
            {
                cart = new List<CartDish>();
                ViewBag.ErrorMessage = ex.Message;
            }
            return View(cart);
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
            await dbContext.Dishes.AddAsync(dish);
            await dbContext.SaveChangesAsync();
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

        [HttpPost]
        public IActionResult RemoveFromCart([FromRoute]int id)
        {
            try
            {
                if (HttpContext.Session.Keys.Contains(SiteInfo.CART_ITEMS_KEY)
                && HttpContext.Session.GetObject<IEnumerable<CartDish>>(SiteInfo.CART_ITEMS_KEY) is List<CartDish> cartItems)
                {
                    if (cartItems.FirstOrDefault(d => d.IdDish == id) is CartDish record)
                        cartItems.Remove(record);
                    HttpContext.Session.SetObject(SiteInfo.CART_ITEMS_KEY, cartItems);

                }
                else
                    ViewBag.ErrorMessage = "Cannot find item with that id";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }
            return RedirectToAction("Cart");
        }

        
        public async Task<IActionResult> AddToCart(int id, int quantity)
        {
            try
            {
                if (HttpContext.Session.Keys.Contains(SiteInfo.CART_ITEMS_KEY)
                && HttpContext.Session.GetObject<IEnumerable<CartDish>>(SiteInfo.CART_ITEMS_KEY) is List<CartDish> cartItems)
                {
                    if (cartItems.FirstOrDefault(d => d.IdDish == id) is CartDish record)
                    {
                        record.Quantity += quantity;
                        cartItems[cartItems.IndexOf(record)] = record;
                    }
                    else{
                        var dish = await dbContext.Dishes.FirstOrDefaultAsync(d => d.IdDish == id);
                        if (dish != null)
                        {
                            var added = (CartDish)dish;
                            added.Quantity = quantity;
                            cartItems.Add(added);
                        }
                    }
                    HttpContext.Session.SetObject(SiteInfo.CART_ITEMS_KEY, cartItems);
                }
                else
                {
                    var dish = await dbContext.Dishes.FirstOrDefaultAsync(d => d.IdDish == id);
                    if (dish != null)
                    {
                        var added = (CartDish)dish;
                        added.Quantity = quantity;

                        HttpContext.Session.SetObject(SiteInfo.CART_ITEMS_KEY, new List<CartDish>
                        {
                            added
                        });
                    }
                    else ViewBag.ErrorMessage = "cannot find dish with that id";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }
            return RedirectToAction("Cart");
        }
    }
}
