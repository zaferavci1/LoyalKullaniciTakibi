using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddZimmetTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Zimmetler",
                columns: table => new
                {
                    ZimmetID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonelID = table.Column<int>(type: "integer", nullable: false),
                    DemirbasAdi = table.Column<string>(type: "text", nullable: false),
                    MarkaModel = table.Column<string>(type: "text", nullable: false),
                    SeriNo = table.Column<string>(type: "text", nullable: false),
                    VerilisTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IadeTarihi = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zimmetler", x => x.ZimmetID);
                    table.ForeignKey(
                        name: "FK_Zimmetler_Personeller_PersonelID",
                        column: x => x.PersonelID,
                        principalTable: "Personeller",
                        principalColumn: "PersonelID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Zimmetler_PersonelID",
                table: "Zimmetler",
                column: "PersonelID");

            migrationBuilder.CreateIndex(
                name: "IX_Zimmetler_SeriNo",
                table: "Zimmetler",
                column: "SeriNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Zimmetler");
        }
    }
}
