using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sustavzapracenjenapretkauteretani.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Planovi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TrajanjeTjedana = table.Column<int>(type: "int", nullable: false),
                    Razina = table.Column<int>(type: "int", nullable: false),
                    DatumKreiranja = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planovi", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vjezbe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MisicnaSkupina = table.Column<int>(type: "int", nullable: false),
                    VideoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RazinaTezine = table.Column<int>(type: "int", nullable: false),
                    PotrebnaOprema = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vjezbe", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Korisnici",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumRodjenja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DatumRegistracije = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Visina = table.Column<double>(type: "float", nullable: false),
                    Tezina = table.Column<double>(type: "float", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Korisnici", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Korisnici_Planovi_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Planovi",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlanVjezba",
                columns: table => new
                {
                    PlanoviId = table.Column<int>(type: "int", nullable: false),
                    VjezbeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanVjezba", x => new { x.PlanoviId, x.VjezbeId });
                    table.ForeignKey(
                        name: "FK_PlanVjezba_Planovi_PlanoviId",
                        column: x => x.PlanoviId,
                        principalTable: "Planovi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanVjezba_Vjezbe_VjezbeId",
                        column: x => x.VjezbeId,
                        principalTable: "Vjezbe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ciljevi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tip = table.Column<int>(type: "int", nullable: false),
                    CiljanaVrijednost = table.Column<double>(type: "float", nullable: false),
                    Rok = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Postignut = table.Column<bool>(type: "bit", nullable: false),
                    KorisnikId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ciljevi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ciljevi_Korisnici_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mjerenja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatumMjerenja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tezina = table.Column<double>(type: "float", nullable: false),
                    PostotakMasti = table.Column<double>(type: "float", nullable: true),
                    OpsegStruka = table.Column<double>(type: "float", nullable: true),
                    KorisnikId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mjerenja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mjerenja_Korisnici_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Treninzi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DatumVrijeme = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrajanjeMinuta = table.Column<int>(type: "int", nullable: false),
                    Vrsta = table.Column<int>(type: "int", nullable: false),
                    Biljeske = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PotroseneKalorije = table.Column<int>(type: "int", nullable: true),
                    Ocjena = table.Column<int>(type: "int", nullable: true),
                    KorisnikId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treninzi", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Treninzi_Korisnici_KorisnikId",
                        column: x => x.KorisnikId,
                        principalTable: "Korisnici",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreningVjezbe",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TreningId = table.Column<int>(type: "int", nullable: false),
                    VjezbaId = table.Column<int>(type: "int", nullable: false),
                    BrojSerija = table.Column<int>(type: "int", nullable: false),
                    BrojPonavljanja = table.Column<int>(type: "int", nullable: false),
                    TezinaKg = table.Column<double>(type: "float", nullable: true),
                    OdmorSekundi = table.Column<int>(type: "int", nullable: false),
                    Redoslijed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreningVjezbe", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreningVjezbe_Treninzi_TreningId",
                        column: x => x.TreningId,
                        principalTable: "Treninzi",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TreningVjezbe_Vjezbe_VjezbaId",
                        column: x => x.VjezbaId,
                        principalTable: "Vjezbe",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ciljevi_KorisnikId",
                table: "Ciljevi",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_Korisnici_PlanId",
                table: "Korisnici",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Mjerenja_KorisnikId",
                table: "Mjerenja",
                column: "KorisnikId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanVjezba_VjezbeId",
                table: "PlanVjezba",
                column: "VjezbeId");

            migrationBuilder.CreateIndex(
                name: "IX_TreningVjezbe_TreningId",
                table: "TreningVjezbe",
                column: "TreningId");

            migrationBuilder.CreateIndex(
                name: "IX_TreningVjezbe_VjezbaId",
                table: "TreningVjezbe",
                column: "VjezbaId");

            migrationBuilder.CreateIndex(
                name: "IX_Treninzi_KorisnikId",
                table: "Treninzi",
                column: "KorisnikId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ciljevi");

            migrationBuilder.DropTable(
                name: "Mjerenja");

            migrationBuilder.DropTable(
                name: "PlanVjezba");

            migrationBuilder.DropTable(
                name: "TreningVjezbe");

            migrationBuilder.DropTable(
                name: "Treninzi");

            migrationBuilder.DropTable(
                name: "Vjezbe");

            migrationBuilder.DropTable(
                name: "Korisnici");

            migrationBuilder.DropTable(
                name: "Planovi");
        }
    }
}
