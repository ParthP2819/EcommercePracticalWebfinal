using Ecommerce.DataAccess.Data;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.ShowAll;

namespace EcommercePractical.Areas.User.Controllers
{
    public class UserController : Controller
    {
        private ApplicationDbContext _db;
        private UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private SignInManager<IdentityUser> _signInManager;
        private IHttpContextAccessor _httpContextAccessor;


        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, SignInManager<IdentityUser> signInManager,
            IHttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
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
            return RedirectToAction("SignUp");
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
            //var result = await _signInManager.PasswordSignInAsync(login.Username, login.Password, false, false);

            var result = await _signInManager.PasswordSignInAsync(user.Email, login.Password, false, false);
            //var userRoles = await _userManager.GetRolesAsync(user);
            if (result.Succeeded)
            {
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
                        Status = "NotOk"
                    });
                }
            }
            else
            {
                ViewBag.NotValidUser = "Invalid credentials:?";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Approve(string email)
        {
            var dealer = _db.dealer.FirstOrDefault(x => x.Email == email);

            IdentityUser user = new()
            {
                Email = dealer.Email,
                UserName = dealer.UserName,
                PhoneNumber = dealer.Phone.ToString(),

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

            return RedirectToAction("Index", "User");
        }

        public IActionResult Reject(string email,string reason)
        {
            var data = _db.dealer.FirstOrDefault(x=>x.Email==email);
            data.status=Status.Reject;
            data.Reason = reason;
            _db.dealer.Update(data);
            _db.SaveChanges();
            return RedirectToAction("Index", "User");
        }
        [HttpGet]
        public IActionResult Popup(string email)
        {
            ViewBag.Email = email;
            return PartialView("_Popup");
        }
    }
}

