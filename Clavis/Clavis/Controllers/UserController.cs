using Clavis.Models;
using Clavis.Paging;
using Clavis.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clavis.Controllers
{
    public class UserController : Controller
    {
        private readonly ClavisDbContext _db;

        public UserController(ClavisDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(
            string sortOrder,
            string numer,
            string access,
            int amount,
            int? pageNumber,
            int pageSize = 4)
        {
            if (HttpContext.Session.GetString("Upr") != "user")
                return RedirectToAction("Index", "Home");

            var result = from s in _db.Rooms select s;

            ViewData["SortParam"] = String.IsNullOrEmpty(sortOrder) ? ViewData["SortParam"] : sortOrder;
            ViewData["searchNumer"] = String.IsNullOrEmpty(numer) ? "" : numer;
            ViewData["searchMiejsca"] = amount;

            if (numer == "")
                numer = ViewData["searchNumer"].ToString();

            switch (sortOrder)
            {
                case "numer":
                    result = result.Where(Room => EF.Functions.Like(Room.Numer, "%" + ViewData["searchNumer"] + "%") && Room.Miejsca >= amount).OrderBy(Room => Room.Numer);
                    break;
                case "numerDesc":
                    result = result.Where(Room => EF.Functions.Like(Room.Numer, "%" + ViewData["searchNumer"] + "%") && Room.Miejsca >= amount).OrderByDescending(Room => Room.Numer);
                    break;
                case "miejsce":
                    result = result.Where(Room => EF.Functions.Like(Room.Numer, "%" + ViewData["searchNumer"] + "%") && Room.Miejsca >= amount).OrderBy(Room => Room.Miejsca);
                    break;
                case "miejsceDesc":
                    result = result.Where(Room => EF.Functions.Like(Room.Numer, "%" + ViewData["searchNumer"] + "%") && Room.Miejsca >= amount).OrderByDescending(Room => Room.Miejsca);
                    break;
                default:
                    result = result.Where(Room => EF.Functions.Like(Room.Numer, "%" + ViewData["searchNumer"] + "%") && Room.Miejsca >= amount).OrderBy(Room => Room.RoomsId);
                    break;
            }
            if (access == "on")
            {
                var temp = from u in _db.Uprawnienia where u.UsersId == HttpContext.Session.GetInt32("Id") select u.RoomsId;
                result = from r in result where temp.Contains(r.RoomsId) select r;
            }


            ViewBag.numer = numer;
            ViewBag.access = access;
            ViewBag.sp = 0;
            ViewBag.miejsca = amount;

            var list = await PaginatedList<Room>.CreateAsync(result.AsNoTracking(), pageNumber ?? 1, pageSize);
            ViewBag.totalPages = list.TotalPages;
            ViewBag.pageIndex = list.PageIndex;
            return View(list);
        }

        public IActionResult PageUp(int page)
        {
            if (HttpContext.Session.GetString("Upr") != "user")
                return RedirectToAction("Index", "Home");
            ViewData["page"] = page + 1;
            return View("Index");
        }

        public IActionResult UserPage()
        {
            if (HttpContext.Session.GetString("Upr") != "user")
                return RedirectToAction("Index", "Home");
            ViewBag.Imie = HttpContext.Session.GetString("Imie");
            if (HttpContext.Session.GetString("New") == "True")
                ViewBag.Warning = "Konto wymaga zmiany hasła!";
            ViewBag.Role = HttpContext.Session.GetString("Role");
            return View("UserPage");
        }

        [HttpGet]
        public IActionResult Room(int room_id,string date,int[] terms)
        {

            if (HttpContext.Session.GetString("Upr") != "user")
                return RedirectToAction("Index", "Home");

            if (date != null)
                ViewBag.SelectedDate = Int32.Parse(date);
            if (terms != null)
                ViewBag.Terms = terms;

            Room room = _db.Rooms.Where(r => r.RoomsId == room_id).FirstOrDefault();
            if (room != null)
            {
                ViewBag.Room = room;
                ViewBag.Error = false;                                    

                if (_db.Uprawnienia.Where(u => u.UsersId == HttpContext.Session.GetInt32("Id") && u.RoomsId == room_id).Count() == 0)
                    ViewBag.Dostepnosc = false;
                else
                    ViewBag.Dostepnosc = true;
            }
            else
                ViewBag.Error = true;

            return View();
        }

        [HttpPost]
        public IActionResult Room(int room_id,string date,string time)
        {
            if (time != null)
            {
                DateTime dateTime = DateTime.Now.Date.AddDays(Int32.Parse(date)).AddHours(7).AddMinutes(90 * Int32.Parse(time));
                var res = _db.Rezerwacjes.Where(re => re.DateFrom == dateTime).FirstOrDefault();
                if(res == null)
                {
                    Rezerwacje rez = new Rezerwacje();
                    rez.DateFrom = dateTime;
                    rez.DateTo = dateTime.AddMinutes(90);
                    rez.RoomsId = room_id;
                    rez.UsersId = HttpContext.Session.GetInt32("Id");
                    rez.Status = 0;
                    _db.Rezerwacjes.Add(rez);
                    _db.SaveChanges();
                    return RedirectToAction("Reservations", "User");
                }               
            }

            ViewBag.SelectedDate = Int32.Parse(date);

            var dateFormat = DateTime.Now.Date.AddDays(Int32.Parse(date));
            var result = from re in _db.Rezerwacjes where re.DateFrom.Date == dateFormat select re;

            int[] terms = { 1, 2, 3, 4, 5, 6, 7, 8 };
            for(int i = 0; i < 8; i++)
            {
                if (result.Where(r => r.DateFrom.TimeOfDay == DateTime.MinValue.AddHours(7).AddMinutes(90 * i).TimeOfDay).Count() > 0)
                    terms[i] = -1;
            }
            return RedirectToAction("Room", "User", new { room_id, date, terms });
        }

        [HttpGet]
        public async Task<IActionResult> Reservations(
            int? historyPageNumber,
            int? currentPageNumber,
            int historyPageSize = 6,            
            int currentPageSize = 6
            )
        {

            if (HttpContext.Session.GetString("Upr") != "user")
                return RedirectToAction("Index", "Home");
            var result = _db.Rezerwacjes.Where(r => r.UsersId == HttpContext.Session.GetInt32("Id") && r.DateTo < DateTime.Now).OrderBy(r=>r.DateFrom).
                Join(_db.Rooms, rez => rez.RoomsId, room => room.RoomsId, (rez, room) => new RezerwacjeView(
                        rez.RezerwacjeId, room.Numer, rez.DateFrom, rez.DateTo, rez.Status));

            var phList = await PaginatedList<RezerwacjeView>.CreateAsync(result, historyPageNumber ?? 1, historyPageSize);
            //List<RezerwacjeView> list = new List<RezerwacjeView>();          
            //var plist = await PaginatedList<RezerwacjeView>.CreateAsync(result, pageNumber ?? 1, pageSize);

            result = _db.Rezerwacjes.Where(r => r.UsersId == HttpContext.Session.GetInt32("Id") && r.DateTo >= DateTime.Now).OrderBy(r => r.DateFrom).
                Join(_db.Rooms, rez => rez.RoomsId, room => room.RoomsId, (rez, room) => new RezerwacjeView(
                        rez.RezerwacjeId, room.Numer, rez.DateFrom, rez.DateTo, rez.Status));

            var pcList = await PaginatedList<RezerwacjeView>.CreateAsync(result, currentPageNumber ?? 1, currentPageSize);
            ViewBag.historyTotalPages = phList.TotalPages;
            ViewBag.historyPageIndex = phList.PageIndex;
            ViewBag.currentTotalPages = pcList.TotalPages;
            ViewBag.currentPageIndex = pcList.PageIndex;
            ViewBag.historyPageList = phList;
            ViewBag.currentPageList = pcList;
            return View("Reservations");
        }

        [HttpGet]
        public IActionResult Account()
        {
            if (HttpContext.Session.GetString("Upr") != "user")
                return RedirectToAction("Index", "Home");
            User user = new User();
            user.Imie = HttpContext.Session.GetString("Imie");
            user.Nazwisko = HttpContext.Session.GetString("Nazwisko");
            user.Email = HttpContext.Session.GetString("Email");
            user.Uprawnienia = HttpContext.Session.GetString("Uprawnienia");
            return View(user);
        }

        [HttpGet]
        public IActionResult changePassword()
        {
            if (HttpContext.Session.GetString("Upr") != "user")
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public IActionResult changePassword(string oldPass,string newPass,string newPassRe)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Login")))
            {
                return RedirectToAction("Index", "Home");
            }
            User loggedInUser = _db.Users.Where(u => u.UsersId == HttpContext.Session.GetInt32("Id")).FirstOrDefault();
            if (loggedInUser != null && BCrypt.Net.BCrypt.Verify(oldPass, loggedInUser.Password))
            {
                if (newPass == null)
                    newPass = "";
                if (newPassRe == null)
                    newPassRe = "";
                if(newPass == newPassRe)
                {
                    if (newPass.Length >= 8)
                    {
                        loggedInUser.Password = BCrypt.Net.BCrypt.HashPassword(newPass);
                        loggedInUser.New = false;
                        HttpContext.Session.SetString("New", "false");
                        _db.Update(loggedInUser);
                        _db.SaveChanges();
                    }
                    else
                    {
                        ViewBag.Message = "Nowe hasło jest za krótkie";
                        return View();
                    }
                }
                else
                {
                    ViewBag.Message = "Hasła nie są takie same";
                    return View();
                }
            }
            else
            {
                ViewBag.Message = "Wprowadzono niepoprawne hasło";
                return View();
            }
            return View();
        }
    }
}
