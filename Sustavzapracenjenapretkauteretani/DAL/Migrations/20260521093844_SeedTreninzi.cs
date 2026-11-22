using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sustavzapracenjenapretkauteretani.Migrations
{
    /// <inheritdoc />
    public partial class SeedTreninzi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Treninzi",
                columns: new[] { "Id", "Biljeske", "DatumVrijeme", "DeletedAt", "KorisnikId", "Ocjena", "PotroseneKalorije", "TrajanjeMinuta", "Vrsta" },
                values: new object[,]
                {
                    { 1, "Odličan trening snage, osjetio se napredak.", new DateTime(2025, 1, 20, 18, 0, 0, 0, DateTimeKind.Unspecified), null, 1, 8, 450, 75, 1 },
                    { 2, "Novi osobni rekord na bench pressu — 90 kg!", new DateTime(2025, 2, 10, 17, 30, 0, 0, DateTimeKind.Unspecified), null, 1, 9, 480, 80, 1 },
                    { 3, "Naporno ali učinkovito za mršavljenje.", new DateTime(2025, 3, 5, 19, 0, 0, 0, DateTimeKind.Unspecified), null, 1, 7, 550, 45, 3 },
                    { 4, "Dobra funkcionalna sesija za cijelo tijelo.", new DateTime(2025, 1, 25, 10, 0, 0, 0, DateTimeKind.Unspecified), null, 2, 7, 320, 60, 4 },
                    { 5, "Lagano trčanje u parku, odlično za odmor.", new DateTime(2025, 2, 20, 9, 30, 0, 0, DateTimeKind.Unspecified), null, 2, 8, 380, 45, 2 },
                    { 6, "Odlično istezanje, osjećam se fleksibilnije.", new DateTime(2025, 3, 15, 11, 0, 0, 0, DateTimeKind.Unspecified), null, 2, 9, 180, 50, 5 },
                    { 7, "Teška sesija za gornji dio tijela, maksimalne težine.", new DateTime(2025, 1, 22, 17, 0, 0, 0, DateTimeKind.Unspecified), null, 3, 9, 620, 90, 1 },
                    { 8, "Kondicionalni trening s visokim intenzitetom.", new DateTime(2025, 2, 18, 18, 0, 0, 0, DateTimeKind.Unspecified), null, 3, 8, 680, 50, 3 },
                    { 9, "Fokus na noge i mrtvo dizanje — solidan napredak.", new DateTime(2025, 3, 20, 17, 30, 0, 0, DateTimeKind.Unspecified), null, 3, 8, 590, 85, 1 }
                });

            migrationBuilder.InsertData(
                table: "TreningVjezbe",
                columns: new[] { "Id", "BrojPonavljanja", "BrojSerija", "DeletedAt", "OdmorSekundi", "Redoslijed", "TezinaKg", "TreningId", "VjezbaId" },
                values: new object[,]
                {
                    { 1, 8, 4, null, 120, 1, 80.0, 1, 1 },
                    { 2, 10, 4, null, 120, 2, 100.0, 1, 2 },
                    { 3, 12, 3, null, 90, 3, null, 1, 7 },
                    { 4, 1, 3, null, 60, 4, null, 1, 5 },
                    { 5, 6, 4, null, 180, 1, 90.0, 2, 1 },
                    { 6, 5, 4, null, 180, 2, 120.0, 2, 3 },
                    { 7, 12, 3, null, 90, 3, 20.0, 2, 4 },
                    { 8, 1, 5, null, 60, 1, null, 3, 6 },
                    { 9, 15, 4, null, 45, 2, null, 3, 8 },
                    { 10, 1, 3, null, 30, 3, null, 3, 5 },
                    { 11, 10, 3, null, 60, 1, null, 4, 8 },
                    { 12, 15, 3, null, 60, 2, null, 4, 2 },
                    { 13, 1, 3, null, 45, 3, null, 4, 5 },
                    { 14, 1, 1, null, 0, 1, null, 5, 6 },
                    { 15, 1, 4, null, 30, 1, null, 6, 5 },
                    { 16, 5, 5, null, 180, 1, 110.0, 7, 1 },
                    { 17, 3, 5, null, 240, 2, 160.0, 7, 3 },
                    { 18, 10, 4, null, 90, 3, 25.0, 7, 4 },
                    { 19, 12, 4, null, 90, 4, null, 7, 7 },
                    { 20, 1, 6, null, 60, 1, null, 8, 6 },
                    { 21, 20, 5, null, 45, 2, null, 8, 8 },
                    { 22, 1, 4, null, 30, 3, null, 8, 5 },
                    { 23, 5, 5, null, 240, 1, 140.0, 9, 2 },
                    { 24, 4, 4, null, 240, 2, 150.0, 9, 3 },
                    { 25, 15, 3, null, 90, 3, null, 9, 7 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "TreningVjezbe",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Treninzi",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Treninzi",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Treninzi",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Treninzi",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Treninzi",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Treninzi",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Treninzi",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Treninzi",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Treninzi",
                keyColumn: "Id",
                keyValue: 9);
        }
    }
}
