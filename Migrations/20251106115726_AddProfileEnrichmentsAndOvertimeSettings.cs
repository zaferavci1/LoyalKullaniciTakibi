using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileEnrichmentsAndOvertimeSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmanID",
                table: "Personeller",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotografDosyaYolu",
                table: "Personeller",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MeslekID",
                table: "Personeller",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Lookup_Departmanlar",
                columns: table => new
                {
                    DepartmanID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tanim = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookup_Departmanlar", x => x.DepartmanID);
                });

            migrationBuilder.CreateTable(
                name: "Lookup_GenelAyarlar",
                columns: table => new
                {
                    AyarKey = table.Column<string>(type: "text", nullable: false),
                    AyarValue = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookup_GenelAyarlar", x => x.AyarKey);
                });

            migrationBuilder.CreateTable(
                name: "Lookup_Meslekler",
                columns: table => new
                {
                    MeslekID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tanim = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookup_Meslekler", x => x.MeslekID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_DepartmanID",
                table: "Personeller",
                column: "DepartmanID");

            migrationBuilder.CreateIndex(
                name: "IX_Personeller_MeslekID",
                table: "Personeller",
                column: "MeslekID");

            migrationBuilder.AddForeignKey(
                name: "FK_Personeller_Lookup_Departmanlar_DepartmanID",
                table: "Personeller",
                column: "DepartmanID",
                principalTable: "Lookup_Departmanlar",
                principalColumn: "DepartmanID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Personeller_Lookup_Meslekler_MeslekID",
                table: "Personeller",
                column: "MeslekID",
                principalTable: "Lookup_Meslekler",
                principalColumn: "MeslekID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personeller_Lookup_Departmanlar_DepartmanID",
                table: "Personeller");

            migrationBuilder.DropForeignKey(
                name: "FK_Personeller_Lookup_Meslekler_MeslekID",
                table: "Personeller");

            migrationBuilder.DropTable(
                name: "Lookup_Departmanlar");

            migrationBuilder.DropTable(
                name: "Lookup_GenelAyarlar");

            migrationBuilder.DropTable(
                name: "Lookup_Meslekler");

            migrationBuilder.DropIndex(
                name: "IX_Personeller_DepartmanID",
                table: "Personeller");

            migrationBuilder.DropIndex(
                name: "IX_Personeller_MeslekID",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "DepartmanID",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "FotografDosyaYolu",
                table: "Personeller");

            migrationBuilder.DropColumn(
                name: "MeslekID",
                table: "Personeller");
        }
    }
}
