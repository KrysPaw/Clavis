using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Clavis.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Numer { get; set; }
        public string Opis { get; set; }
        public bool Vacant { get; set; }
    }
}
