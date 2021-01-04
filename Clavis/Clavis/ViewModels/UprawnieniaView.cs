using Clavis.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clavis.ViewModels
{
    public class UprawnieniaView
    {

        public UprawnieniaView(int _id,User _user,Room _room)
        {
            ID = _id;
            user = _user;
            room = _room;
        }

        public int ID { get; set; }
        public User user { get; set; }
        public Room room { get; set; }
        
    }
}
