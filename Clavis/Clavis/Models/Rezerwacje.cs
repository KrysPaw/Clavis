using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Clavis.Models
{
    [Table("rezerwacje")]
    public partial class Rezerwacje
    {
        [Key]
        [Column("rezerwacje_id")]
        public int RezerwacjeId { get; set; }
        [Column("rooms_id")]
        public int? RoomsId { get; set; }
        [Column("users_id")]
        public int? UsersId { get; set; }
        [Column("date_from", TypeName = "datetime")]
        public DateTime? DateFrom { get; set; }
        [Column("date_to", TypeName = "datetime")]
        public DateTime? DateTo { get; set; }
        [Column("status")]
        public int? Status { get; set; }

        [ForeignKey(nameof(RoomsId))]
        [InverseProperty(nameof(Room.Rezerwacjes))]
        public virtual Room Rooms { get; set; }
        [ForeignKey(nameof(UsersId))]
        [InverseProperty(nameof(User.Rezerwacjes))]
        public virtual User Users { get; set; }
    }
}
