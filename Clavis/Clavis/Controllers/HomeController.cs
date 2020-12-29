using Clavis.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;

namespace Clavis.Controllers
{
    public class HomeController : Controller
    {

        private readonly ClavisDbContext _db;

        public HomeController(ClavisDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Login")))
                return View();
            else
                return RedirectToAction("UserPage", "User");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Login")))
                return View();
            else
                return RedirectToAction("UserPage", "User");
        }


        [HttpPost]
        public IActionResult Login(User user)
        {

            Debug.WriteLine(BCrypt.Net.BCrypt.HashPassword(user.Password));
            User loggedInUser = _db.Users.SingleOrDefault(x => x.Login == user.Login);
            if (loggedInUser == null)
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

            Response.Cookies.Append("LastLoggedInTime", DateTime.Now.ToString());
            return RedirectToAction("UserPage", "User");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}