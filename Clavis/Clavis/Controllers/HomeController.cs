using Clavis.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PagedList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Clavis.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        private readonly ClavisDbContext _db;

        public HomeController(ClavisDbContext db)                
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Login()
        {          
            return View();
        }


        [HttpPost]
        public IActionResult Login(User user)
        {
            User loggedInUser = _db.Users.Where(x => x.Login == user.Login && x.Password == user.Password).FirstOrDefault();
            Debug.WriteLine("Login : "+user.Login + "\n Password : " + user.Password);
            if(loggedInUser == null)
            {
                ViewBag.Message = "Nieprawidłowy login lub hasło.";
                return View();
            }
            ViewBag.Message = "Zalogowano";

            HttpContext.Session.SetString("Login", loggedInUser.Login); 
            HttpContext.Session.SetString("Imie", loggedInUser.Imie); 
            HttpContext.Session.SetString("Nazwisko", loggedInUser.Nazwisko); 
            HttpContext.Session.SetString("Email", loggedInUser.Email); 
            HttpContext.Session.SetString("Upr", loggedInUser.Uprawnienia); 

            return RedirectToAction("UserPage","User");
        }

        public IActionResult Register(string username, string password)
        {
            return RedirectToAction("Index");
        }
        
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}