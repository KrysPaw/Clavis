using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace Clavis.Models
{
    public partial class ClavisDbContext : DbContext
    {
        public ClavisDbContext()
        {
        }

        public ClavisDbContext(DbContextOptions<ClavisDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Rezerwacje> Rezerwacjes { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<Uprawnienium> Uprawnienia { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rezerwacje>(entity =>
            {
                entity.HasOne(d => d.Rooms)
                    .WithMany(p => p.Rezerwacjes)
                    .HasForeignKey(d => d.RoomsId)
                    .HasConstraintName("FK_rezerwacje_rooms");

                entity.HasOne(d => d.Users)
                    .WithMany(p => p.Rezerwacjes)
                    .HasForeignKey(d => d.UsersId)
                    .HasConstraintName("FK_rezerwacje_users");
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.RoomsId)
                    .HasName("PK__rooms__80B6BBB3A1E37442");

                entity.Property(e => e.Numer).IsUnicode(false);

                entity.Property(e => e.Opis).IsUnicode(false);

                entity.Property(e => e.Uwagi).IsUnicode(false);
            });

            modelBuilder.Entity<Uprawnienium>(entity =>
            {
                entity.HasKey(e => e.UprawnieniaId)
                    .HasName("PK__uprawnie__5C4D723B347EB8D5");

                entity.HasOne(d => d.Rooms)
                    .WithMany(p => p.Uprawnienia)
                    .HasForeignKey(d => d.RoomsId)
                    .HasConstraintName("FK_uprawnienia_rooms");

                entity.HasOne(d => d.Users)
                    .WithMany(p => p.UprawnieniaNavigation)
                    .HasForeignKey(d => d.UsersId)
                    .HasConstraintName("FK_uprawnienia_users");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UsersId)
                    .HasName("PK__users__EAA7D14B5D9CBA18");

                entity.Property(e => e.Email).IsUnicode(false);

                entity.Property(e => e.Imie).IsUnicode(false);

                entity.Property(e => e.Login).IsUnicode(false);

                entity.Property(e => e.Nazwisko).IsUnicode(false);

                entity.Property(e => e.Password).IsFixedLength(true);

                entity.Property(e => e.Uprawnienia).IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
