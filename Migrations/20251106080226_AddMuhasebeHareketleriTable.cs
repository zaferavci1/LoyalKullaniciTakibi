using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddMuhasebeHareketleriTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lookup_MuhasebeHareketTipi",
                columns: table => new
                {
                    HareketTipiID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tanim = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookup_MuhasebeHareketTipi", x => x.HareketTipiID);
                });

            migrationBuilder.CreateTable(
                name: "MuhasebeHareketleri",
                columns: table => new
                {
                    HareketID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelID = table.Column<int>(type: "integer", nullable: false),
                    HareketTipiID = table.Column<int>(type: "integer", nullable: false),
                    Tutar = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Aciklama = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuhasebeHareketleri", x => x.HareketID);
                    table.ForeignKey(
                        name: "FK_MuhasebeHareketleri_Lookup_MuhasebeHareketTipi_HareketTipiID",
                        column: x => x.HareketTipiID,
                        principalTable: "Lookup_MuhasebeHareketTipi",
                        principalColumn: "HareketTipiID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MuhasebeHareketleri_Personeller_PersonelID",
                        column: x => x.PersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeHareketleri_HareketTipiID",
                table: "MuhasebeHareketleri",
                column: "HareketTipiID");

            migrationBuilder.CreateIndex(
                name: "IX_MuhasebeHareketleri_PersonelID",
                table: "MuhasebeHareketleri",
                column: "PersonelID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MuhasebeHareketleri");

            migrationBuilder.DropTable(
                name: "Lookup_MuhasebeHareketTipi");
        }
    }
}
