using Ecommerce.DataAccess;
using Ecommerce.DataAccess.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Win32;
using MimeKit;
using System.Drawing;
using System.Dynamic;
using static Ecommerce.Models.ShowAll;

namespace EcommercePractical.Areas.User.Controllers
{
    public class UserController   : Controller
    {
        private ApplicationDbContext _db;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private SignInManager<ApplicationUser> _signInManager;
        private IHttpContextAccessor _httpContextAccessor;
        private IEmailSender _emailSender;



        public UserController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager,
            IHttpContextAccessor httpContextAccessor, IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _emailSender = emailSender;
        }
        
        public async Task<IActionResult> Index()
        {
            if(!User.Identity.IsAuthenticated)
            {
                return View("page");
            }
            //ViewBag.Role = HttpContext.Session.GetString("user");

            var user = await _userManager.GetUserAsync(User );
             var role = await _userManager.GetRolesAsync(user);
            var curentRole=role.FirstOrDefault();
            //var curentRole= ViewBag.Role;
            ViewBag.Role = curentRole;


            dynamic obj = new ExpandoObject();
            var dealerlist =await _userManager.GetUsersInRoleAsync(Roles.Dealer.ToString());
            
            if (curentRole == Roles.SuperAdmin.ToString())
            {
                obj.dealer = dealerlist;
                obj.admin = await _userManager.GetUsersInRoleAsync("Admin");
            }

            if (curentRole == Roles.Admin.ToString())
            {
                obj.dealer = dealerlist;
            }
            if(curentRole == Roles.Dealer.ToString())
            {
                if (user.Status == Status.Approves)
                {
                    obj.product = _db.product.ToList();
                }
                else
                {
                    HttpContext.Session.Clear();
                    return RedirectToAction("Login");
                }
            }
           
            return View(obj);
        }

        public IActionResult SignUp()
        {
            //IEnumerable<Dealer> dealerList = _db.dealer.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp(Register register)
        {
            if (register.Password != register.ConfirmPassword)
            {
                ViewBag.NotValidUser = "Both Password should match";
                return View("SignUp");
            }
            //var dealer = _db.dealer.FirstOrDefault(x => x.Email == register.Email);
            //if (dealer != null) 
            //{
            //    ViewBag.NotValidUser = "User already Exists:/";
            //    return View("SignUp");
            //}
         
            ApplicationUser user = new ()
            {
                Email = register.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = register.UserName,
                PhoneNumber = register.PhoneNumber,
                StreetAddress = register.StreetAddress,
                City = register.City,
                PostalCode = register.PostalCode,
                Country = register.Country,
                State = register.State,
                Status = ShowAll.Status.Pending,
                Reason = ""
                

            };

            var result = await _userManager.CreateAsync(user,register.Password);
            if(result.Succeeded){
            ViewBag.NotValidUser = "Admin will accept your email in short time:";
                await _userManager.AddToRoleAsync(user, Roles.Dealer.ToString());
                //var registerUser =  await _signInManager.PasswordSignInAsync(register.Email, register.Password, false, false);
                return RedirectToAction("Login");
              
            }
            //if(await _roleManager.RoleExistsAsync(Roles.Admin))
            return View();
        }

        public IActionResult AddAdmin()
        {
            return View("SignUp");
        }

        [HttpPost]
        public async Task<IActionResult> AddAdmin(Register register) //Roles roles)
        {
            var userExists = await _userManager.FindByNameAsync(register.UserName);
            if (userExists != null)
                return Ok("User already exists!");

            ApplicationUser user = new ApplicationUser()
            {
                Email = register.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = register.UserName,
                PhoneNumber = register.PhoneNumber,
                StreetAddress = register.StreetAddress,
                City = register.City,
                PostalCode = register.PostalCode,
                Country = register.Country,
                State = register.State,
                Status = ShowAll.Status.Pending,
                Reason = ""

            };
            var result = await _userManager.CreateAsync(user, register.Password);
            if (!result.Succeeded)
                return Ok("User creation failed! Please check user details and try again.");

            if (await _roleManager.RoleExistsAsync(Roles.Admin.ToString()))
            {
                await _userManager.AddToRoleAsync(user, Roles.Admin.ToString());
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {
            var user = await _userManager.FindByEmailAsync(login.Email);          
            var result = await _signInManager.PasswordSignInAsync(user.UserName, login.Password, false, false);
            var temp = await _signInManager.CheckPasswordSignInAsync(user, login.Password,false);
            var Roledata = await _userManager.GetRolesAsync(user);
            var cRole = Roledata.FirstOrDefault();
            ViewBag.Role = cRole;
            if (result.Succeeded && temp.Succeeded)
            {
                // HttpContext.Response.Cookies.Append("user", user.Email);
                HttpContext.Session.SetString("user", cRole);
                return RedirectToAction("Index");
            }
            //else if(!result.Succeeded)
            //{
            //    HttpContext.Session.SetString("user", cRole);
            //    return RedirectToAction("Index", "Product");
            //}
            return Ok("Invalid");
           
        }
        
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            HttpContext.Session.Clear();

            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Approve(string email)
        {
            var dealer = await _userManager.FindByEmailAsync(email);
            dealer.Status= ShowAll.Status.Approves;                      
           
            await _db.SaveChangesAsync();
            // send mail
            var message = new Message(new string[] { dealer.Email }, "Test email", "This is the content from our email.");
            _emailSender.SendEmail(message);


            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Reject(string email, string reason)
        {
            var data = await _userManager.FindByEmailAsync(email);
            data.Status = Status.Reject;
            data.Reason = reason;
            _db.Update(data);
            _db.SaveChanges();

            // send mail
            var message = new Message(new string[] { "panchalparth7122@gmail.com" }, "Test email", "This is the content from our email.");
            _emailSender.SendEmail(message);

            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Popup(string email)
        {
            ViewBag.Email = email;
            return PartialView("_Popup");
        }
        public async Task<IActionResult> Block(string email)
        {

            var user= await _userManager.FindByEmailAsync(email);
            user.IsActive = false;



            user.Status = Status.Block;
             await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Unblock(string email)
        {

            var user = await _userManager.FindByEmailAsync(email);
            user.IsActive = true;

            user.Status = Status.Approves;

            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        //if (result.Succeeded || user.IsActive==true )
        //{
        //     HttpContext.Response.Cookies.Append("user", user.Email);    
        //    if (user.UserName == "SuperAdmin" || user.UserName == "Admin")
        //    {
        //        return RedirectToAction("Index", "User");
        //    }
        //    else
        //    {
        //        return BadRequest(new ResponseModel
        //        {
        //            Message = "Invalid Credentials",
        //            Data = user,
        //            Status = "Not Found"
        //        });
        //    }
        //}
        //else
        //{
        //    ViewBag.NotValidUser = "Invalid credentials:?";
        //    return RedirectToAction("Index");
        //}
    }
}

