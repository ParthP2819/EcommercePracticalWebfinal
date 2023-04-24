using Ecommerce.DataAccess.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using static Ecommerce.Models.ShowAll;

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
                    obj.ImageUrl =  @"\images\products\" + fileName + extension;
                }
                if (obj.Id == 0)
                {
                    _db.Add(obj);
                    TempData["success"] = "Product Create Successfully";
                }
                else
                {
                    _db.Update(obj);
                    TempData["success"] = "Product Updated Successfully";
                }
                await _db.SaveChangesAsync();
                
                return RedirectToAction("Index","User");
            }
            return View(obj);

          
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
            var product = _db.product.Find(id); 

            if (product.ImageUrl != null)
            {
                var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            _db.product.Remove(product); 
            _db.SaveChanges();
            //TempData["success"] = "Product deleted successfully";
            return RedirectToAction("Index", "User");

        }
       
        public async Task<IActionResult> Active(int id)
        {
            var product = _db.product.Find(id);

            if (product.IsActive == true)
            {
                product.IsActive = false;
            }
            else
            {
                product.IsActive = true;
            }
            _db.product.Update(product);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "User");
        }

        public async Task<IActionResult> DeActive(int id)
        {
            var product = _db.product.Find(id);

            if (product.IsActive == false)
            {
                product.IsActive = true;
            }
            else
            {
                product.IsActive = false;
            }
            _db.product.Update(product);
            await _db.SaveChangesAsync();
            return RedirectToAction("Index", "User");
        }

        //Add Discount
        public IActionResult AddDiscount(int id)
        {
            ViewBag.Id = id;
            //ViewBag.pid = id;
            return View();
        }

        public IActionResult AddDiscount(Discount dis)
        {
            _db.discount.Add(dis);

            var product = _db.product.Find(dis.Id);

            if (dis.FromDate > dis.ToDate)
            {
                ModelState.AddModelError("obj.ToDate",
                                         "ToDate must be greater than Fromdate.");
                return View(dis);
            }
            if (dis.DiscountType.ToString() == "Amount")
            {
                if(product.Price < dis.Amount)
                {
                    ModelState.AddModelError("obj.Amount",
                                             "Amount can't be greater than Product Actual Price.");
                    return View(dis);
                }
                product.DiscountAmount = product.Price - dis.Amount;
            }
            else
            {
                if (dis.Amount > 100)
                {
                    ModelState.AddModelError("obj.Amount",
                                             "Discount can't be greater than 100%.");
                    return View(dis);
                }
                product.DiscountAmount = product.Price - ((product.Price * dis.Amount) / 100);
            }


            //if (dis.DiscountType == DiscountType.Amount)
            //{
            //    product.DiscountAmount = dis.Amount; /*product.Price - dis.Amount;*/

            //}
            //else
            //{
            //    product.DiscountAmount = (product.Price * dis.Amount) / 100;

            //}
            _db.product.Update(product);
            _db.SaveChanges();
            return RedirectToAction("Index","User");
        }

    }
}
