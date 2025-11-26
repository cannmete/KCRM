using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KCRM.Migrations
{
    /// <inheritdoc />
    public partial class updatingNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        UPDATE Notes n
        SET UserId = 1 
        WHERE NOT EXISTS (SELECT 1 FROM Users u WHERE u.Id = n.UserId);
    ");
            migrationBuilder.Sql("UPDATE Notes SET UserId = 1 WHERE UserId IS NULL;");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Users_UserId",
                table: "Notes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Customers_CustomerId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Users_UserId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_CustomerId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_UserId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Notes");
        }
    }
}
