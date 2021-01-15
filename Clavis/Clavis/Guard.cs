using Clavis.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Clavis
{
    public class Guard
    {
        private readonly ClavisDbContext _db;
        public Guard(ClavisDbContext db)
        {
            _db = db;
        }
        public void checkStatuses()
        {
            var result = _db.Rezerwacjes.Where(re => re.DateTo > DateTime.Now && re.Status == 2).ToList();
            for (int i = 0; i < result.Count(); i++)
            {
                result[i].Status = 4;
            }
            _db.Rezerwacjes.UpdateRange(result);
        }
    }
}
