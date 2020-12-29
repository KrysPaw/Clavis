using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Clavis.Models
{
    [Table("rooms")]
    public partial class Room
    {
        public Room()
        {
            Rezerwacjes = new HashSet<Rezerwacje>();
            Uprawnienia = new HashSet<Uprawnienium>();
        }

        [Key]
        [Column("rooms_id")]
        public int RoomsId { get; set; }
        [Column("numer")]
        [StringLength(10)]
        public string Numer { get; set; }
        [Column("opis")]
        [StringLength(200)]
        public string Opis { get; set; }
        [Column("miejsca")]
        public int? Miejsca { get; set; }
        [Column("uwagi")]
        [StringLength(200)]
        public string Uwagi { get; set; }

        [InverseProperty(nameof(Rezerwacje.Rooms))]
        public virtual ICollection<Rezerwacje> Rezerwacjes { get; set; }
        [InverseProperty(nameof(Uprawnienium.Rooms))]
        public virtual ICollection<Uprawnienium> Uprawnienia { get; set; }
    }
}
