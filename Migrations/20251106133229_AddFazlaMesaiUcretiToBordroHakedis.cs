using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoyalKullaniciTakip.Migrations
{
    /// <inheritdoc />
    public partial class AddFazlaMesaiUcretiToBordroHakedis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FazlaMesaiUcreti",
                table: "BordroHakedisKayitlari",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FazlaMesaiUcreti",
                table: "BordroHakedisKayitlari");
        }
    }
}
