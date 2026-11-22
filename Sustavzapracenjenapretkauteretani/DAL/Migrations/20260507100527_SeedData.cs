using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sustavzapracenjenapretkauteretani.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Planovi",
                columns: new[] { "Id", "DatumKreiranja", "Naziv", "Opis", "Razina", "TrajanjeTjedana" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Početni plan", "Idealan za početnike koji tek kreću s vježbanjem.", 1, 8 },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Snaga i masa", "Plan usmjeren na povećanje mišićne mase i jakosti.", 2, 12 },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Napredni program", "Intenzivan plan za iskusne vježbače.", 3, 16 }
                });

            migrationBuilder.InsertData(
                table: "Vjezbe",
                columns: new[] { "Id", "MisicnaSkupina", "Naziv", "Opis", "PotrebnaOprema", "RazinaTezine", "VideoUrl" },
                values: new object[,]
                {
                    { 1, 1, "Bench Press", "Vježba za prsa na ravnoj klupi s šipkom ili bučicama.", true, 3, null },
                    { 2, 6, "Čučnjevi", "Temeljna vježba za noge i gluteus.", false, 2, null },
                    { 3, 2, "Mrtvo dizanje", "Složena vježba za leđa, noge i ruke.", true, 4, null },
                    { 4, 4, "Biceps curl", "Izolirana vježba za biceps s bučicama ili šipkom.", true, 2, null },
                    { 5, 7, "Plank", "Statička vježba za stabilizaciju trbuha i trupa.", false, 1, null },
                    { 6, 8, "Trčanje", "Aerobna aktivnost za kardiovaskularni sustav.", false, 1, null },
                    { 7, 5, "Triceps dips", "Vježba za triceps na paralelnim šipkama.", true, 2, null },
                    { 8, 1, "Sklekovi", "Vježba s vlastitom težinom za prsa i triceps.", false, 1, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Planovi",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Planovi",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Planovi",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 8);
        }
    }
}
