using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sustavzapracenjenapretkauteretani.Migrations
{
    /// <inheritdoc />
    public partial class SeedKorisniciICiljevi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Korisnici",
                columns: new[] { "Id", "DatumRegistracije", "DatumRodjenja", "Email", "Ime", "PlanId", "Prezime", "Tezina", "Visina" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1995, 3, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "marko.horvat@email.com", "Marko", 2, "Horvat", 88.0, 182.0 },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1998, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "ana.kovac@email.com", "Ana", 1, "Kovač", 62.0, 165.0 },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1990, 11, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "tomislav.blazic@email.com", "Tomislav", 3, "Blažić", 95.0, 178.0 }
                });

            migrationBuilder.InsertData(
                table: "Ciljevi",
                columns: new[] { "Id", "CiljanaVrijednost", "KorisnikId", "Postignut", "Rok", "Tip" },
                values: new object[,]
                {
                    { 1, 80.0, 1, false, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 2, 120.0, 1, false, new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3 },
                    { 3, 57.0, 2, true, new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1 },
                    { 4, 90.0, 2, false, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5 },
                    { 5, 100.0, 3, false, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 },
                    { 6, 60.0, 3, false, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
