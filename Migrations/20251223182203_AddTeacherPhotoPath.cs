using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RSWEB.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherPhotoPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Teachers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Teachers");
        }
    }
}
