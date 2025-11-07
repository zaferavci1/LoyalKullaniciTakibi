using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddBordroHakedisKaydiTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BordroHakedisKayitlari",
                columns: table => new
                {
                    KayitID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelID = table.Column<int>(type: "integer", nullable: false),
                    Yil = table.Column<int>(type: "integer", nullable: false),
                    Ay = table.Column<int>(type: "integer", nullable: false),
                    HesaplananTarih = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    BrutMaas = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    KesintiTutari = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetHakedis = table.Column<decimal>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BordroHakedisKayitlari", x => x.KayitID);
                    table.ForeignKey(
                        name: "FK_BordroHakedisKayitlari_Personeller_PersonelID",
                        column: x => x.PersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BordroHakedisKayitlari_PersonelID_Yil_Ay",
                table: "BordroHakedisKayitlari",
                columns: new[] { "PersonelID", "Yil", "Ay" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BordroHakedisKayitlari");
        }
    }
}
