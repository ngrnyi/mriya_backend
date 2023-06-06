using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessengerBackend.Migrations
{
    /// <inheritdoc />
    public partial class ischeckedUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "wasChecked",
                table: "Messages",
                newName: "isChecked");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isChecked",
                table: "Messages",
                newName: "wasChecked");
        }
    }
}
