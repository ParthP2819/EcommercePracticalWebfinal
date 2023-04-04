using Ecommerce.DataAccess.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EcommercePractical.Areas.User.Controllers
{
    public class ProductController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;


        public ProductController(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager,ApplicationDbContext db)
        {
            _db= db;
            _userManager= userManager;
            _roleManager= roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? id)
        {
            return View();
            var user = await _userManager.GetUserAsync(User);
            var role = await _userManager.GetRolesAsync(user);
            var currentrole = role.FirstOrDefault();
            ViewBag.roletype = currentrole;
            if (id == null)
            {
                var userd = await _userManager.GetUserAsync(User);
                ViewBag.UserName = user.Email;
                var data = _db.product.Where(x => x.CreatedBy == userd.Id).ToList();
                return View(data);
            }
            else if (id != null)
            {
                var userdata = await _userManager.FindByEmailAsync(id);
                var data = _db.product.Where(x => x.CreatedBy == userdata.Id).ToList();
                return View(data);
            }
            else
            {
                return View();
            }
        }
       
        [HttpGet]
        public IActionResult AddProduct(int? id) 
        {
            if (id == null)
            {
                return View();
            }
            else
            {
                var data = _db.product.Find(id);
                return View(data);
            }
        }
        public async Task<IActionResult> AddProduct(Product product)
        {
            var data = await _userManager.GetUserAsync(User);
            product.CreatedBy = data.Id;
            await _db.product.AddAsync(product);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        //Edit Product
        [HttpPost]
        public IActionResult EditProduct(Product product)
        {
            _db.product.Update(product);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        //Delete Product
        public IActionResult DeleteProduct(int id)
        {
            var data = _db.product.Find(id);
            _db.product.Remove(data);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        ////Add Discount
        //public IActionResult AddDiscount(int id)
        //{
        //    ViewBag.pid = id;
        //    return View();
        //}
        //[HttpPost]
        //public IActionResult AddDiscount(Discount discount)
        //{
        //    _db.discount.Add(discount);

        //    var product = _db.product.Find(discount.ProductId);

        //    if (discount.DiscountType == DiscountType.Amount)
        //    {
        //        product.DiscountAmount = discount.Amount;
        //    }
        //    else
        //    {
        //        product.DiscountAmount = (product.Price * discount.Amount) / 100;

        //    }
        //    _db.product.Update(product);
        //    _db.SaveChanges();
        //    return RedirectToAction("Index");
        //}
    }
}
