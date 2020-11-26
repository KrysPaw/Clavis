using Clavis.Data;
using Clavis.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
       
        private DbData data;

        public HomeController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,IConfiguration config)
        {
            _userManager = userManager;
            data = new DbData(config);
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

        public IActionResult Login()
        {          
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {

            //login functionality
            var user = _userManager.FindByNameAsync(username);

            if(user != null)
            {
                var signInResult = await _signInManager.PasswordSignInAsync(username, password, false, false);

                if (signInResult.Succeeded)
                {
                    return RedirectToAction("Index");
                }
            }
            return RedirectToAction("Index");
        }

        public IActionResult Register(string username, string password)
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> LogOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index");
        }


        public IActionResult RoomList()
        {           
            return View(data.getRooms("",0,"none"));//todo
        }
        [HttpPost]
        public IActionResult RoomList(string numer,bool access,int amount,string sort,int? page)
        {
            if (numer == null)
                numer = "";
            int pageSize = 3;
            var result = data.getRooms(numer, amount, sort);
            var pageResult = result.ToPagedList(page ?? 1, pageSize);
            ViewBag.maxPages = Math.Ceiling((decimal)result.Count/pageSize);
            ViewBag.numer = numer;
            ViewBag.access = access;
            ViewBag.miejsca = amount;            
            return View(pageResult);            
        }

    }
}