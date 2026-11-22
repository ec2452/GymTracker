using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Sustavzapracenjenapretkauteretani.Migrations
{
    /// <inheritdoc />
    public partial class SeedMjerenja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Mjerenja",
                columns: new[] { "Id", "DatumMjerenja", "DeletedAt", "KorisnikId", "OpsegStruka", "PostotakMasti", "Tezina" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, 95.0, 22.0, 88.0 },
                    { 2, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, 93.0, 21.0, 86.0 },
                    { 3, new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, 90.0, 20.0, 84.0 },
                    { 4, new DateTime(2025, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 1, 88.0, 19.0, 82.0 },
                    { 5, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, 72.0, 24.0, 62.0 },
                    { 6, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, 70.0, 23.0, 61.0 },
                    { 7, new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, 68.0, 22.0, 59.0 },
                    { 8, new DateTime(2025, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2, 65.0, 21.0, 57.0 },
                    { 9, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 3, 110.0, 28.0, 95.0 },
                    { 10, new DateTime(2025, 2, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 3, 112.0, 27.0, 97.0 },
                    { 11, new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 3, 115.0, 26.0, 99.0 },
                    { 12, new DateTime(2025, 4, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 3, 118.0, 25.0, 101.0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Mjerenja",
                keyColumn: "Id",
                keyValue: 12);
        }
    }
}
