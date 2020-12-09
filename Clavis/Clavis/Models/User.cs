using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace Clavis.Models
{
    [Table("users")]
    public partial class User
    {
        public User()
        {
            Rezerwacjes = new HashSet<Rezerwacje>();
            UprawnieniaNavigation = new HashSet<Uprawnienium>();
        }

        [Key]
        [Column("users_id")]
        public int UsersId { get; set; }
        [Column("imie")]
        [StringLength(30)]
        public string Imie { get; set; }
        [Column("nazwisko")]
        [StringLength(30)]
        public string Nazwisko { get; set; }
        [Column("email")]
        [StringLength(50)]
        public string Email { get; set; }
        [Column("login")]
        [StringLength(30)]
        public string Login { get; set; }
        [Column("password")]
        [MaxLength(64)]
        public byte[] Password { get; set; }
        [Required]
        [Column("uprawnienia")]
        [StringLength(20)]
        public string Uprawnienia { get; set; }

        [InverseProperty(nameof(Rezerwacje.Users))]
        public virtual ICollection<Rezerwacje> Rezerwacjes { get; set; }
        [InverseProperty(nameof(Uprawnienium.Users))]
        public virtual ICollection<Uprawnienium> UprawnieniaNavigation { get; set; }
    }
}
