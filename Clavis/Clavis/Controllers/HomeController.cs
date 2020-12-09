using Clavis.Models;
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


        public IActionResult RoomList(int page = 1,int pageSize = 3)
        {
            PagedList<Room> pagedResult = new PagedList<Room>(_db.Rooms, page, pageSize);
            return View("RoomList",pagedResult);

        }
        [HttpPost]
        public IActionResult RoomList(string numer,bool access,int amount,string sort,int page = 1, int pageSize = 3)
        {
            
            if (numer == null)
                numer = "";
            IQueryable<Room> result;
            switch (sort)
            {
                case "numUp":
                    result = _db.Rooms.Where(Room => EF.Functions.Like(Room.Numer, "%" + numer + "%") && Room.Miejsca >= amount).OrderBy(Room => Room.Numer);
                    break;
                case "numDown":
                    result = _db.Rooms.Where(Room => EF.Functions.Like(Room.Numer, "%" + numer + "%") && Room.Miejsca >= amount).OrderByDescending(Room => Room.Numer);
                    break;
                case "mieUp":
                    result = _db.Rooms.Where(Room => EF.Functions.Like(Room.Numer, "%" + numer + "%") && Room.Miejsca >= amount).OrderBy(Room => Room.Miejsca);
                    break;
                case "mieDown":
                    result = _db.Rooms.Where(Room => EF.Functions.Like(Room.Numer, "%" + numer + "%") && Room.Miejsca >= amount).OrderByDescending(Room => Room.Miejsca);
                    break;
                default:
                    result = _db.Rooms.Where(Room => EF.Functions.Like(Room.Numer, "%" + numer + "%") && Room.Miejsca >= amount);
                    break;
            }
            List<Room> listResult = result.ToList();
            PagedList<Room> pagedResult = new PagedList<Room>(listResult, page, pageSize);
            ViewBag.page = page;
            ViewBag.maxPages = Math.Ceiling((decimal)listResult.Count/pageSize);
            ViewBag.numer = numer;
            ViewBag.access = access;
            ViewBag.sp = 0;
            ViewBag.miejsca = amount;    
            
            return View("RoomList",pagedResult);            
        }

    }
}