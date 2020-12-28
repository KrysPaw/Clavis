using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clavis.Models;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using Clavis.Paging;
using Microsoft.AspNetCore.Http;

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
            bool access, 
            int amount, 
            int? pageNumber , 
            int pageSize = 4)
        {
            //if (numer == null)
            //    numer = "";

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
            ViewData["page"] = page+1;
            return View("Index");
        }

        public IActionResult UserPage()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Login")))
            {
                return RedirectToAction("Login", "Home");
            }
            ViewBag.Imie = HttpContext.Session.GetString("Imie");
            ViewBag.Role = HttpContext.Session.GetString("Role");
            return View("UserPage");
        }
    }
}
