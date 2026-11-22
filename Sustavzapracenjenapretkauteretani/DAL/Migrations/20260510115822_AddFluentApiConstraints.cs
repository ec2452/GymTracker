using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sustavzapracenjenapretkauteretani.Migrations
{
    /// <inheritdoc />
    public partial class AddFluentApiConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VideoUrl",
                table: "Vjezbe",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "PotrebnaOprema",
                table: "Vjezbe",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Naziv",
                table: "Vjezbe",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Biljeske",
                table: "Treninzi",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "TezinaKg",
                table: "TreningVjezbe",
                type: "float(6)",
                precision: 6,
                scale: 2,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Opis",
                table: "Planovi",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Naziv",
                table: "Planovi",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DatumKreiranja",
                table: "Planovi",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<double>(
                name: "Tezina",
                table: "Mjerenja",
                type: "float(5)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "PostotakMasti",
                table: "Mjerenja",
                type: "float(4)",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "OpsegStruka",
                table: "Mjerenja",
                type: "float(6)",
                precision: 6,
                scale: 1,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Tezina",
                table: "Korisnici",
                type: "float(5)",
                precision: 5,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Korisnici",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "Postignut",
                table: "Ciljevi",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<double>(
                name: "CiljanaVrijednost",
                table: "Ciljevi",
                type: "float(10)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.CreateIndex(
                name: "IX_Vjezbe_Naziv",
                table: "Vjezbe",
                column: "Naziv",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_Email",
                table: "Korisnici",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vjezbe_Naziv",
                table: "Vjezbe");

            migrationBuilder.DropIndex(
                name: "IX_Korisnici_Email",
                table: "Korisnici");

            migrationBuilder.AlterColumn<string>(
                name: "VideoUrl",
                table: "Vjezbe",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "PotrebnaOprema",
                table: "Vjezbe",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Naziv",
                table: "Vjezbe",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Biljeske",
                table: "Treninzi",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "TezinaKg",
                table: "TreningVjezbe",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float(6)",
                oldPrecision: 6,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Opis",
                table: "Planovi",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Naziv",
                table: "Planovi",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DatumKreiranja",
                table: "Planovi",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<double>(
                name: "Tezina",
                table: "Mjerenja",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(5)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<double>(
                name: "PostotakMasti",
                table: "Mjerenja",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float(4)",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "OpsegStruka",
                table: "Mjerenja",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float(6)",
                oldPrecision: 6,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Tezina",
                table: "Korisnici",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(5)",
                oldPrecision: 5,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Korisnici",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<bool>(
                name: "Postignut",
                table: "Ciljevi",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<double>(
                name: "CiljanaVrijednost",
                table: "Ciljevi",
                type: "float",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(10)",
                oldPrecision: 10,
                oldScale: 2);
        }
    }
}
