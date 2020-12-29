using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Clavis.Models
{
    [Table("uprawnienia")]
    public partial class Uprawnienium
    {
        [Key]
        [Column("uprawnienia_id")]
        public int UprawnieniaId { get; set; }
        [Column("rooms_id")]
        public int? RoomsId { get; set; }
        [Column("users_id")]
        public int? UsersId { get; set; }

        [ForeignKey(nameof(RoomsId))]
        [InverseProperty(nameof(Room.Uprawnienia))]
        public virtual Room Rooms { get; set; }
        [ForeignKey(nameof(UsersId))]
        [InverseProperty(nameof(User.UprawnieniaNavigation))]
        public virtual User Users { get; set; }
    }
}
