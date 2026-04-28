using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace story_maker_api.Migrations
{
    /// <inheritdoc />
    public partial class AddCoverImageUrlToStory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImageUrl",
                table: "Stories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoverImageUrl",
                table: "Stories");
        }
    }
}
