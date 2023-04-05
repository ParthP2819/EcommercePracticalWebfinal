using Ecommerce.DataAccess.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace EcommercePractical.Areas.User.Controllers
{
    public class ProductController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductController(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager, ApplicationDbContext db, IWebHostEnvironment hostEnvironment)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _hostEnvironment = hostEnvironment;
        }

        //[HttpGet]
        //public async Task<IActionResult> Index(string? id)
        //{
        //    return View();
        //    var user = await _userManager.GetUserAsync(User);
        //    var role = await _userManager.GetRolesAsync(user);
        //    var currentrole = role.FirstOrDefault();
        //    ViewBag.roletype = currentrole;
        //    if (id == null)
        //    {
        //        var userd = await _userManager.GetUserAsync(User);
        //        ViewBag.UserName = user.Email;
        //        var data = _db.product.Where(x => x.CreatedBy == userd.Id).ToList();
        //        return View(data);
        //    }
        //    else if (id != null)
        //    {
        //        var userdata = await _userManager.FindByEmailAsync(id);
        //        var data = _db.product.Where(x => x.CreatedBy == userdata.Id).ToList();
        //        return View(data);
        //    }
        //    else
        //    {
        //        return View();
        //    }
        //}

        [HttpGet]
        public IActionResult AddProduct(int? id) 
        {
            var prod = new Product();
            if (id == null)
            {
                return View(prod);
            }
            else
            {
                var data = _db.product.Find(id);
                return View(data);
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product obj,IFormFile? file)
        {
            var data = await _userManager.GetUserAsync(User);
            obj.CreatedBy = data.Id;

            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(wwwRootPath, @"images\products");
                    var extension = Path.GetExtension(file.FileName);

                    if (obj.ImageUrl != null)
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension), FileMode.Create))
                    {
                        file.CopyTo(fileStreams);
                    }
                    obj.ImageUrl = /*wwwRootPath +*/ @"\images\products\" + fileName + extension;
                }
                if (obj.Id == 0)
                {
                    _db.Add(obj);
                }
                else
                {
                    _db.Update(obj);
                }
                await _db.SaveChangesAsync();
                //TempData["success"] = "Product Create Successfully";
                return RedirectToAction("Index","User");
            }
            return View(obj);

            //await _db.product.AddAsync(obj);
            //await _db.SaveChangesAsync();
            //return RedirectToAction("Index","User");
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
