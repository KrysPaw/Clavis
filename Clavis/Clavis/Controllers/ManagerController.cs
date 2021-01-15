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
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Clavis.Controllers
{
    public class ManagerController : Controller
    {
        private readonly ClavisDbContext _db;

        public ManagerController(ClavisDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult MainPage()
        {
            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Users(
            string sortOrder,
            string filter,
            string onlyUsers,
            int? pageNumber,
            int pageSize = 8
            )
        {

            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");

            var result = from u in _db.Users select u;

            ViewData["SortParam"] = String.IsNullOrEmpty(sortOrder) ? ViewData["SortParam"] : sortOrder;
            ViewData["Filter"] = String.IsNullOrEmpty(filter) ? "" : filter;
            ViewData["OnlyUsers"] = onlyUsers;


            result = result.Where(u=>( EF.Functions.Like(u.Nazwisko,ViewData["Filter"] + "%") || EF.Functions.Like(u.Imie,ViewData["Filter"] + "%")) && u.UsersId!=HttpContext.Session.GetInt32("Id"));
            switch (sortOrder)
            {
                case "identyfikator":
                    result = result.OrderBy(u => u.UsersId);
                    break;
                case "identyfikatorDesc":
                    result = result.OrderByDescending(u => u.UsersId);
                    break;
                case "nazwisko":
                    result = result.OrderBy(u => u.Nazwisko);
                    break;
                case "nazwiskoDesc":
                    result = result.OrderByDescending(u => u.Nazwisko);
                    break;
            }

            if (onlyUsers != null)
            {
                result = result.Where(u => u.Uprawnienia == "user");
            }

            var list = await PaginatedList<User>.CreateAsync(result.AsNoTracking(), pageNumber ?? 1, pageSize);
            ViewBag.totalPages = list.TotalPages;
            ViewBag.pageIndex = list.PageIndex;
            return View(list);
        }

        [HttpGet]
        public IActionResult UserAdd()
        {
            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public IActionResult UserAdd(User user,string accountType)
        {
            user.Imie = string.IsNullOrEmpty(user.Imie) ? "" : user.Imie;
            user.Nazwisko = string.IsNullOrEmpty(user.Nazwisko) ? "" : user.Nazwisko;
            user.Login = string.IsNullOrEmpty(user.Login) ? "" : user.Login;
            user.Email = string.IsNullOrEmpty(user.Email) ? "" : user.Email;

            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");
            bool valid = true;
            if(user.Imie.Length==0)
            {
                valid = false;
                ViewBag.Message = "Pole imienia nie może być puste.";
            }
            if(user.Imie.Any(char.IsDigit))
            {
                valid = false;
                ViewBag.Message = "Pole imienia nie może zawierać cyfr.";
            }
            if (user.Nazwisko.Length == 0)
            {
                valid = false;
                ViewBag.Message = "Pole nazwiska nie może być puste.";
            }
            if (user.Nazwisko.Any(char.IsDigit))
            {
                valid = false;
                ViewBag.Message = "Pole nazwiska nie może zawierać cyfr.";
            }
            if (user.Login.Length < 5)
            {
                valid = false;
                ViewBag.Message = "Login nie może być krótszy niż 5 znaków.";
            }
            if (Regex.IsMatch(user.Email, @"^([\w\.\-] +)@([\w\-] +)((\.(\w){ 2,3})+)$"))
            {
                valid = false;
                ViewBag.Message = "Niepoprawny adres email.";
            }
            if (valid)
            {
                user.New = true;
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Email);
                user.Uprawnienia = accountType;
                _db.Users.Add(user);
                _db.SaveChanges();
                ViewBag.Message = "Pomyślnie utworzono nowe konto! Wprowadzony adres email jest tymczasowym hasłem dostępu.";               
            }
            else
            {
                ViewBag.Login = user.Login;
                ViewBag.Email = user.Email;
                ViewBag.Imie = user.Imie;
                ViewBag.Nazwisko = user.Nazwisko;
            }
            return View();
        }

        [HttpGet]
        public IActionResult User(bool badId, int user_id)
        {
            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");
            var user = _db.Users.Where(u => u.UsersId == user_id).FirstOrDefault();
            ViewBag.Error = false;
            if (user == null)
            {
                ViewBag.Error = true;
            }
            if (badId == true)
                ViewBag.BadId = true;
            return View(user);
        }

        [HttpPost]
        public IActionResult DeleteUser(int confId,int user_id)
        {
            if (confId == user_id)
            {
                var user = _db.Users.Where(r => r.UsersId == user_id).FirstOrDefault();
                var reservations = _db.Rezerwacjes.Where(re => re.UsersId == user_id);
                _db.Rezerwacjes.RemoveRange(reservations);
                _db.Users.Remove(user);
                _db.SaveChanges();
            }
            else
            {
                return RedirectToAction("User", "Manager", new { user_id = user_id, badId = true });
            }

            return RedirectToAction("Users", "Manager");
        }

        [HttpGet]
        public async Task<IActionResult> Reservations(
            string sortOrder,
            string filter,
            string currentOnly,
            int? pageNumber,
            int pageSize = 6
            )
        {
            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");

            ViewData["SortParam"] = String.IsNullOrEmpty(sortOrder) ? ViewData["SortParam"] : sortOrder;
            ViewData["CurrentOnly"] = String.IsNullOrEmpty(currentOnly) ? ViewData["CurrentOnly"] : currentOnly;
            ViewData["Filter"] = String.IsNullOrEmpty(filter) ? "" : filter;

            DateTime refTime = new DateTime();
            if (currentOnly == "on")
                refTime = DateTime.Now;
            else
                refTime = DateTime.MinValue.AddYears(1780);

            if (sortOrder == "data")
            {
                var result = from re in _db.Rezerwacjes where re.DateFrom > refTime
                             join ro in _db.Rooms on re.RoomsId equals ro.RoomsId
                             join us in _db.Users on re.UsersId equals us.UsersId
                             where EF.Functions.Like(ro.Numer, "%" + ViewData["Filter"] + "%") || EF.Functions.Like(us.Nazwisko, ViewData["Filter"] + "%") 
                             orderby re.DateFrom
                             select new RezerwacjeView(re.RezerwacjeId, us, ro, re.DateFrom, re.DateTo, re.Status);
                var list = await PaginatedList<RezerwacjeView>.CreateAsync(result, pageNumber ?? 1, pageSize);
                ViewBag.totalPages = list.TotalPages;
                ViewBag.pageIndex = list.PageIndex;
                return View(list);
            }
            if(sortOrder == "dataDesc")
            {
                var result = from re in _db.Rezerwacjes where re.DateFrom > refTime
                             join ro in _db.Rooms on re.RoomsId equals ro.RoomsId
                             join us in _db.Users on re.UsersId equals us.UsersId
                             where EF.Functions.Like(ro.Numer, "%" + ViewData["Filter"] + "%") || EF.Functions.Like(us.Nazwisko, ViewData["Filter"] + "%") 
                             orderby re.DateFrom descending
                             select new RezerwacjeView(re.RezerwacjeId, us, ro, re.DateFrom, re.DateTo, re.Status);
                var list = await PaginatedList<RezerwacjeView>.CreateAsync(result, pageNumber ?? 1, pageSize);
                ViewBag.totalPages = list.TotalPages;
                ViewBag.pageIndex = list.PageIndex;
                return View(list);
            }
            if (sortOrder == "user")
            {
                var result = from re in _db.Rezerwacjes where re.DateFrom > refTime
                             join ro in _db.Rooms on re.RoomsId equals ro.RoomsId
                             join us in _db.Users on re.UsersId equals us.UsersId
                             where EF.Functions.Like(ro.Numer, "%" + ViewData["Filter"] + "%") || EF.Functions.Like(us.Nazwisko, ViewData["Filter"] + "%") 
                             orderby us.Nazwisko
                             select new RezerwacjeView(re.RezerwacjeId, us, ro, re.DateFrom, re.DateTo, re.Status);
                var list = await PaginatedList<RezerwacjeView>.CreateAsync(result, pageNumber ?? 1, pageSize);
                ViewBag.totalPages = list.TotalPages;
                ViewBag.pageIndex = list.PageIndex;
                return View(list);
            }
            if (sortOrder == "userDesc")
            {
                var result = from re in _db.Rezerwacjes where re.DateFrom > refTime
                             join ro in _db.Rooms on re.RoomsId equals ro.RoomsId
                             join us in _db.Users on re.UsersId equals us.UsersId
                             where EF.Functions.Like(ro.Numer, "%" + ViewData["Filter"] + "%") || EF.Functions.Like(us.Nazwisko, ViewData["Filter"] + "%")
                             orderby us.Nazwisko descending
                             select new RezerwacjeView(re.RezerwacjeId, us, ro, re.DateFrom, re.DateTo, re.Status);
                var list = await PaginatedList<RezerwacjeView>.CreateAsync(result, pageNumber ?? 1, pageSize);
                ViewBag.totalPages = list.TotalPages;
                ViewBag.pageIndex = list.PageIndex;
                return View(list);
            }
            if (sortOrder == "room")
            {
                var result = from re in _db.Rezerwacjes where re.DateFrom > refTime
                             join ro in _db.Rooms on re.RoomsId equals ro.RoomsId
                             join us in _db.Users on re.UsersId equals us.UsersId
                             where EF.Functions.Like(ro.Numer, "%" + ViewData["Filter"] + "%") || EF.Functions.Like(us.Nazwisko, ViewData["Filter"] + "%")
                             orderby ro.Numer
                             select new RezerwacjeView(re.RezerwacjeId, us, ro, re.DateFrom, re.DateTo, re.Status);
                var list = await PaginatedList<RezerwacjeView>.CreateAsync(result, pageNumber ?? 1, pageSize);
                ViewBag.totalPages = list.TotalPages;
                ViewBag.pageIndex = list.PageIndex;
                return View(list);
            }
            if (sortOrder == "roomDesc")
            {
                var result = from re in _db.Rezerwacjes where re.DateFrom > refTime
                             join ro in _db.Rooms on re.RoomsId equals ro.RoomsId
                             join us in _db.Users on re.UsersId equals us.UsersId
                             where EF.Functions.Like(ro.Numer, "%" + ViewData["Filter"] + "%") || EF.Functions.Like(us.Nazwisko, ViewData["Filter"] + "%")
                             orderby ro.Numer descending
                             select new RezerwacjeView(re.RezerwacjeId, us, ro, re.DateFrom, re.DateTo, re.Status);
                var list = await PaginatedList<RezerwacjeView>.CreateAsync(result, pageNumber ?? 1, pageSize);
                ViewBag.totalPages = list.TotalPages;
                ViewBag.pageIndex = list.PageIndex;
                return View(list);
            }
            var res = from re in _db.Rezerwacjes where re.DateFrom > refTime
                         join ro in _db.Rooms on re.RoomsId equals ro.RoomsId
                         join us in _db.Users on re.UsersId equals us.UsersId
                         where EF.Functions.Like(ro.Numer, "%" + ViewData["Filter"] + "%") || EF.Functions.Like(us.Nazwisko, ViewData["Filter"] + "%")
                         select new RezerwacjeView(re.RezerwacjeId, us, ro, re.DateFrom, re.DateTo, re.Status);
            var lis = await PaginatedList<RezerwacjeView>.CreateAsync(res, pageNumber ?? 1, pageSize);
            ViewBag.totalPages = lis.TotalPages;
            ViewBag.pageIndex = lis.PageIndex;
            return View(lis);

        }

        [HttpGet]
        public IActionResult Reservation(int rez_id)
        {
            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");

            var result = from re in _db.Rezerwacjes                        
                         join ro in _db.Rooms on re.RoomsId equals ro.RoomsId
                         join us in _db.Users on re.UsersId equals us.UsersId
                         where re.RezerwacjeId == rez_id
                         select new RezerwacjeView(re.RezerwacjeId, us, ro, re.DateFrom, re.DateTo, re.Status);
            ViewBag.Error = false;
            if (result.Count() == 0)
            {
                ViewBag.Error = true;
            }
            return View(result.FirstOrDefault());
        }

        [HttpPost]
        public IActionResult Reservation(int rez_id,int status)
        {
            var rez = _db.Rezerwacjes.Where(re => re.RezerwacjeId == rez_id).FirstOrDefault();
            if (rez != null)
            {
                rez.Status = status;
                _db.Rezerwacjes.Update(rez);
                _db.SaveChanges();
                return RedirectToAction("Reservation","Manager",new { rez_id = rez_id});
            }

            return RedirectToAction("Reservation", "Manager", new { rez_id = rez_id });
        }

        [HttpGet]
        public IActionResult Room(int room_id, bool badId)
        {
            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");

            Room room = _db.Rooms.Where(r => r.RoomsId == room_id).FirstOrDefault();
            if (room != null)
            {
                ViewBag.Room = room;
                ViewBag.Error = false;
            }
            else
                ViewBag.Error = true;

            if (badId == true)
                ViewBag.BadId = true;

            return View();
        }

        [HttpGet]
        public IActionResult RoomEdit(int room_id)
        {
            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");

            Room room = _db.Rooms.Where(r => r.RoomsId == room_id).FirstOrDefault();
            if (room != null)
            {
                ViewBag.Room = room;
                ViewBag.Error = false;
            }
            else
                ViewBag.Error = true;

            return View(room);
        }

        [HttpPost]
        public IActionResult RoomEdit(Room room,int room_id)
        {
            room.RoomsId = room_id;
            _db.Rooms.Update(room);
            _db.SaveChanges();

            return RedirectToAction("Room", "Manager",new { room_id = room.RoomsId});
        }

        [HttpPost]
        public IActionResult RoomDelete(int confId,int room_id)
        {
            if (confId == room_id)
            {
                var room = _db.Rooms.Where(r => r.RoomsId == room_id).FirstOrDefault();
                var reservations = _db.Rezerwacjes.Where(re => re.RoomsId == room_id);
                _db.Rezerwacjes.RemoveRange(reservations);
                _db.Rooms.Remove(room);
                _db.SaveChanges();
            }
            else
            {
                return RedirectToAction("Room","Manager",new { room_id = room_id, badId = true});
            }
            
            return RedirectToAction("Rooms", "Manager");
        }

        [HttpGet]
        public async Task<IActionResult> Rooms(string sortOrder,
            string numer,
            int amount,
            int? pageNumber,
            int pageSize = 4)
        {

            if (HttpContext.Session.GetString("Upr") != "manager")
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


            ViewBag.numer = numer;
            ViewBag.sp = 0;
            ViewBag.miejsca = amount;

            var list = await PaginatedList<Room>.CreateAsync(result.AsNoTracking(), pageNumber ?? 1, pageSize);
            ViewBag.totalPages = list.TotalPages;
            ViewBag.pageIndex = list.PageIndex;
            return View(list);
        }

        [HttpGet]
        public IActionResult RoomAdd()
        {
            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");
            return View("RoomAdd",new Room());
        }

        [HttpPost]
        public IActionResult RoomAdd(Room room)
        {
            bool valid = true;
            if(room.Numer==null || room.Numer.Length == 0)
            {
                ViewBag.Message = "Pole numer nie może być puste";
                valid = false;
            }
            if(room.Miejsca==null || room.Miejsca <= 0)
            {
                ViewBag.Message = "Ilość miejsc musi być większa od 0";
                valid = false;
            }
            if (valid)
            {
                _db.Rooms.Add(room);
                _db.SaveChanges();
                ViewBag.Message = "Pomyślnie dodano nową sale";
                return View(new Room());
            }
            return View("RoomAdd",room);
        }

        [HttpGet]
        public IActionResult Account()
        {
            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");
            User user = new User();
            user.Imie = HttpContext.Session.GetString("Imie");
            user.Nazwisko = HttpContext.Session.GetString("Nazwisko");
            user.Email = HttpContext.Session.GetString("Email");
            user.Uprawnienia = HttpContext.Session.GetString("Uprawnienia");
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Permissions( 
            int user_id,
            bool badId,
            int? pageNumber,
            int pageSize = 6
            )
        {
            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");

            if (badId == true)
                ViewBag.BadId = true;
            var user = _db.Users.Where(us => us.UsersId == user_id && us.Uprawnienia == "user").FirstOrDefault();
            if (user != null)
            {
                var result = from up in _db.Uprawnienia
                             join ro in _db.Rooms on up.RoomsId equals ro.RoomsId
                             where up.UsersId == user_id
                             select new UprawnieniaView(up.UprawnieniaId, user, ro);

                var list = await PaginatedList<UprawnieniaView>.CreateAsync(result.AsNoTracking(), pageNumber ?? 1, pageSize);
                ViewBag.User = user;
                ViewBag.totalPages = list.TotalPages;
                ViewBag.pageIndex = list.PageIndex;
                return View(list);
            }
            return RedirectToAction("MainPage", "Manager");
        }

        [HttpGet]
        public IActionResult PermissionAdd(int user_id)
        {

            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");
            var user = _db.Users.Where(us => us.UsersId == user_id && us.Uprawnienia=="user").FirstOrDefault();

            if (user != null)
            {
                /*
                var result = _db.Rooms.ToList();

                foreach(var item in _db.Rooms.ToList())
                {
                    if (_db.Uprawnienia.Where(up => up.RoomsId == item.RoomsId && up.UsersId == user_id).Count() > 0)
                        result.Remove(item);
                }
                */

                var result = from room in _db.Rooms where !_db.Uprawnienia.Any(up => up.UsersId == user_id && up.RoomsId == room.RoomsId) select room;

                ViewBag.User = user;
                if (result != null)
                {
                    return View(result);
                }
            }            
            return RedirectToAction("Permissions","Manager",new { user_id = user_id });
        }

        [HttpPost]
        public IActionResult PermissionAdd(int user_id,int room_id)
        {
            Uprawnienium upr = new Uprawnienium();
            upr.UsersId = user_id;
            upr.RoomsId = room_id;
            _db.Uprawnienia.Add(upr);
            _db.SaveChanges();
            return RedirectToAction("Permissions", "Manager", new { user_id = user_id });
        }

        [HttpPost]
        public IActionResult PermissionDelete(int user_id,int permission_id)
        {
            var perm = _db.Uprawnienia.Where(u => u.UprawnieniaId == permission_id).FirstOrDefault();
            if (perm != null)
            {
                _db.Uprawnienia.Remove(perm);
                _db.SaveChanges();
            }                    
            else
            {
                return RedirectToAction("Permissions", "Manager", new { user_id = user_id , badId = true});
            }
            return RedirectToAction("Permissions", "Manager",new { user_id = user_id });
        }

        [HttpGet]
        public IActionResult changePassword()
        {
            if (HttpContext.Session.GetString("Upr") != "manager")
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public IActionResult changePassword(string oldPass, string newPass, string newPassRe)
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
                if (newPass == newPassRe)
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
