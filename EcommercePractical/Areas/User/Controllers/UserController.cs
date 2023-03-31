using Ecommerce.DataAccess;
using Ecommerce.DataAccess.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index()
        {
            IEnumerable<Dealer> dealerList = _db.dealer.ToList();
            return View(dealerList);
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
            var dealer = _db.dealer.FirstOrDefault(x => x.Email == register.Email);
            if (dealer != null)
            {
                ViewBag.NotValidUser = "User already Exists:/";
                return View("SignUp");
            }

            Dealer dealerNew = new Dealer()
            {
                Email = register.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = register.UserName,
                Phone = register.PhoneNumber,
                StreetAddress = register.StreetAddress,
                City = register.City,
                PostalCode = register.PostalCode,
                Country = "India",
                State = register.State,
                status = ShowAll.Status.Pending,
                Password = register.Password,
                Reason = ""

            };
            await _db.dealer.AddAsync(dealerNew);
            await _db.SaveChangesAsync();
            ViewBag.NotValidUser = "Admin will accept your email in short time:";
            //var registerUser =  await _signInManager.PasswordSignInAsync(register.Email, register.Password, false, false);
            return RedirectToAction("Login");
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
            var result = await _signInManager.PasswordSignInAsync(user.UserName, login.Password, false, true);
            
            if (result.Succeeded || user.IsActive==true )
            {
                 HttpContext.Response.Cookies.Append("user", user.Email);    
                if (user.UserName == "SuperAdmin" || user.UserName == "Admin")
                {
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    return BadRequest(new ResponseModel
                    {
                        Message = "Invalid Credentials",
                        Data = user,
                        Status = "Not Found"
                    });
                }
            }
            else
            {
                ViewBag.NotValidUser = "Invalid credentials:?";
                return RedirectToAction("Index");
            }
        }
        
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            HttpContext.Response.Cookies.Delete("user");
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Approve(string email)
        {
            var dealer = _db.dealer.FirstOrDefault(x => x.Email == email);

            ApplicationUser user = new()
            {
                Email = dealer.Email,
                UserName = dealer.UserName,
                PhoneNumber = dealer.Phone.ToString(),
                City= dealer.City,
                State = dealer.State,
                Country = dealer.Country,
                PostalCode= dealer.PostalCode,
                IsActive = true,
                StreetAddress= dealer.StreetAddress,
                

                

            };
            var result = await _userManager.CreateAsync(user, dealer.Password);

            if (result.Succeeded)
            {
                if (!await _roleManager.RoleExistsAsync(Roles.Dealer.ToString())) //(Roles.Dealer.ToString()))
                {
                    await _roleManager.CreateAsync(new IdentityRole(Roles.Dealer.ToString()));
                }
                await _userManager.AddToRoleAsync(user, Roles.Dealer.ToString());
            }

            dealer.status = Status.Approves;
            _db.SaveChanges();


            // send mail
            var message = new Message(new string[] { dealer.Email }, "Test email", "This is the content from our email.");
            _emailSender.SendEmail(message);


            return RedirectToAction("Index", "User");
        }

        public IActionResult Reject(string email, string reason)
        {
            var data = _db.dealer.FirstOrDefault(x => x.Email == email);
            data.status = Status.Reject;
            data.Reason = reason;
            _db.dealer.Update(data);
            _db.SaveChanges();

            // send mail
            var message = new Message(new string[] { "panchalparth7122@gmail.com" }, "Test email", "This is the content from our email.");
            _emailSender.SendEmail(message);

            return RedirectToAction("Index", "User");
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

            var data = _db.dealer.FirstOrDefault(x => x.Email == email);
             data.status=Status.Block;

            _db.dealer.Update(data);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "User");
        }
        public async Task<IActionResult> Unblock(string email)
        {

            var user = await _userManager.FindByEmailAsync(email);
            user.IsActive = true;

            var data = _db.dealer.FirstOrDefault(x => x.Email == email);
            data.status = Status.Pending;

            _db.dealer.Update(data);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "User");
        }
    }
}

