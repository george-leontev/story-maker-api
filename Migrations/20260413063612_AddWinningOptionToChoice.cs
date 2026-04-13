using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace story_maker_api.Migrations
{
    /// <inheritdoc />
    public partial class AddWinningOptionToChoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WinningOption",
                table: "Choices",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WinningOption",
                table: "Choices");
        }
    }
}
