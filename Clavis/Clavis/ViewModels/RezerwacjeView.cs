using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clavis.ViewModels
{
    public class RezerwacjeView
    {
        public RezerwacjeView(int id,string numer,DateTime from, DateTime to, string status)
        {
            ID = id;
            RoomNumer = numer;
            DateFrom = from;
            DateTo = to;
            Status = status;
        }

        public int ID { get; set; }
        public string RoomNumer { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Status { get; set; }
        public int StatusInt { get; set; }
        public DateTime DateReturn { get; set; }

    }
}
