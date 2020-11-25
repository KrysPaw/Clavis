using Clavis.Data;
using Clavis.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clavis.PageModels
{
    public class RoomListModel : PageModel
    {
        private readonly AppDbContext _db;

        public RoomListModel(AppDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Room> getRooms { get; set; }

        public async Task OnGet()
        {
            getRooms = await _db.rooms.ToListAsync();
        }
    }
}
