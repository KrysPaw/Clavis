using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Clavis.Models
{
    public class Room
    {
        [Key]
        public int rooms_id { get; set; }
        public string numer { get; set; }
        public string opis { get; set; }
        public int miejsca { get; set; }
    }
}
