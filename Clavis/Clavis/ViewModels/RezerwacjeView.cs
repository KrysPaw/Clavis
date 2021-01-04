using Clavis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clavis.ViewModels
{
    public class RezerwacjeView
    {
        public RezerwacjeView(int id,string numer,DateTime from, DateTime to, int status)
        {
            ID = id;
            RoomNumer = numer;
            DateFrom = from;
            DateTo = to;
            StatusId = status;
            switch (StatusId)
            {
                case 0: Status = "Zaakceptowana"; break;
                case 1: Status = "Odrzucona"; break;
                case 2: Status = "Wydano klucze"; break;
                case 3: Status = "Zakończona"; break;
                case 4: Status = "Oczekiwanie na zwrot kluczy"; break;
            }
        }
        public RezerwacjeView(int id, User _user, Room _room, DateTime from, DateTime to, int status)
        {
            ID = id;
            room =_room;
            user = _user;
            DateFrom = from;
            DateTo = to;
            StatusId = status;
            switch (StatusId)
            {
                case 0: Status = "Zaakceptowana"; break;
                case 1: Status = "Odrzucona"; break;
                case 2: Status = "Wydano klucze"; break;
                case 3: Status = "Zakończona"; break;
                case 4: Status = "Oczekiwanie na zwrot kluczy"; break;
            }
        }

        public int ID { get; set; }
        public string RoomNumer { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public string Status { get; set; }
        public int StatusId { get; set; }
        public DateTime DateReturn { get; set; }

        public User user { get; set; }
        public Room room { get; set; }

    }
}
