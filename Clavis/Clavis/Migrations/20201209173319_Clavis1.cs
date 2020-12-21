using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Clavis.Migrations
{
    public partial class Clavis1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    rooms_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    numer = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    opis = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true),
                    miejsca = table.Column<int>(type: "int", nullable: true),
                    uwagi = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__rooms__80B6BBB3A1E37442", x => x.rooms_id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    users_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    imie = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    nazwisko = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    login = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: true),
                    password = table.Column<byte[]>(type: "binary(64)", fixedLength: true, maxLength: 64, nullable: true),
                    uprawnienia = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__EAA7D14B5D9CBA18", x => x.users_id);
                });

            migrationBuilder.CreateTable(
                name: "rezerwacje",
                columns: table => new
                {
                    rezerwacje_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rooms_id = table.Column<int>(type: "int", nullable: true),
                    users_id = table.Column<int>(type: "int", nullable: true),
                    date_from = table.Column<DateTime>(type: "datetime", nullable: true),
                    date_to = table.Column<DateTime>(type: "datetime", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rezerwacje", x => x.rezerwacje_id);
                    table.ForeignKey(
                        name: "FK_rezerwacje_rooms",
                        column: x => x.rooms_id,
                        principalTable: "rooms",
                        principalColumn: "rooms_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rezerwacje_users",
                        column: x => x.users_id,
                        principalTable: "users",
                        principalColumn: "users_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "uprawnienia",
                columns: table => new
                {
                    uprawnienia_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    rooms_id = table.Column<int>(type: "int", nullable: true),
                    users_id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__uprawnie__5C4D723B347EB8D5", x => x.uprawnienia_id);
                    table.ForeignKey(
                        name: "FK_uprawnienia_rooms",
                        column: x => x.rooms_id,
                        principalTable: "rooms",
                        principalColumn: "rooms_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_uprawnienia_users",
                        column: x => x.users_id,
                        principalTable: "users",
                        principalColumn: "users_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rezerwacje_rooms_id",
                table: "rezerwacje",
                column: "rooms_id");

            migrationBuilder.CreateIndex(
                name: "IX_rezerwacje_users_id",
                table: "rezerwacje",
                column: "users_id");

            migrationBuilder.CreateIndex(
                name: "IX_uprawnienia_rooms_id",
                table: "uprawnienia",
                column: "rooms_id");

            migrationBuilder.CreateIndex(
                name: "IX_uprawnienia_users_id",
                table: "uprawnienia",
                column: "users_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rezerwacje");

            migrationBuilder.DropTable(
                name: "uprawnienia");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
