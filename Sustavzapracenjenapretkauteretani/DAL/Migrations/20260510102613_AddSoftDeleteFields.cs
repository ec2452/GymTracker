using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sustavzapracenjenapretkauteretani.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Vjezbe",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Treninzi",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "TreningVjezbe",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Planovi",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Mjerenja",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Korisnici",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Ciljevi",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 1,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 2,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 3,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 4,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 5,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ciljevi",
                keyColumn: "Id",
                keyValue: 6,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 1,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 2,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Korisnici",
                keyColumn: "Id",
                keyValue: 3,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Planovi",
                keyColumn: "Id",
                keyValue: 1,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Planovi",
                keyColumn: "Id",
                keyValue: 2,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Planovi",
                keyColumn: "Id",
                keyValue: 3,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 1,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 2,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 3,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 4,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 5,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 6,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 7,
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "Vjezbe",
                keyColumn: "Id",
                keyValue: 8,
                column: "DeletedAt",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Vjezbe");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Treninzi");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "TreningVjezbe");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Planovi");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Mjerenja");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Korisnici");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Ciljevi");
        }
    }
}
