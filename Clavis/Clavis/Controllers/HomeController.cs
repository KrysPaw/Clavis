using Clavis.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Clavis.Controllers
{
    public class HomeController : Controller
    {

        private readonly ClavisDbContext _db;

        public HomeController(ClavisDbContext db)
        {
            _db = db;
            /*
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(1);
            Guard guard = new Guard(_db);
            var timer = new Timer((e) =>
            {
                guard.checkStatuses();
            }, null, startTimeSpan, periodTimeSpan);
            */
        }

        public IActionResult Index()
        {
            switch (HttpContext.Session.GetString("Upr"))
            {
                case "user": return RedirectToAction("UserPage", "User");
                case "manager": return RedirectToAction("MainPage", "Manager");
                case "admin": return RedirectToAction("MainPage", "Admin");
                default: return View();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Login()
        {
            switch (HttpContext.Session.GetString("Upr"))
            {
                case "user": return RedirectToAction("UserPage", "User");
                case "manager": return RedirectToAction("MainPage", "Manager");
                case "admin": return RedirectToAction("MainPage", "Admin");
                default: return View();
            }
        }


        [HttpPost]
        public IActionResult Login(User user)
        {
            User loggedInUser = _db.Users.SingleOrDefault(x => x.Login == user.Login);
            if (loggedInUser == null || user.Password == null || user.Login == null)
            {
                ViewBag.Message = "Nieprawidłowy login lub hasło.";
                return View();
            }
            bool validPass = BCrypt.Net.BCrypt.Verify(user.Password, loggedInUser.Password);
            if (!validPass)
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
            HttpContext.Session.SetInt32("Id", loggedInUser.UsersId);
            HttpContext.Session.SetString("New", loggedInUser.New.ToString());

            Response.Cookies.Append("LastLoggedInTime", DateTime.Now.ToString());
            switch (HttpContext.Session.GetString("Upr"))
            {
                case "user": return RedirectToAction("UserPage", "User");
                case "manager": return RedirectToAction("MainPage", "Manager");
                case "admin": return RedirectToAction("MainPage", "Admin");
                default: return View();
            }
            return RedirectToAction("Index", "Home");
            
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}