using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmanIDToPuantajGunluk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DepartmanID",
                table: "PuantajGunluk",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PuantajGunluk_DepartmanID",
                table: "PuantajGunluk",
                column: "DepartmanID");

            migrationBuilder.AddForeignKey(
                name: "FK_PuantajGunluk_Lookup_Departmanlar_DepartmanID",
                table: "PuantajGunluk",
                column: "DepartmanID",
                principalTable: "Lookup_Departmanlar",
                principalColumn: "DepartmanID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PuantajGunluk_Lookup_Departmanlar_DepartmanID",
                table: "PuantajGunluk");

            migrationBuilder.DropIndex(
                name: "IX_PuantajGunluk_DepartmanID",
                table: "PuantajGunluk");

            migrationBuilder.DropColumn(
                name: "DepartmanID",
                table: "PuantajGunluk");
        }
    }
}
