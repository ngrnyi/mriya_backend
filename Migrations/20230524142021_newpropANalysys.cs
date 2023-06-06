using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessengerBackend.Migrations
{
    /// <inheritdoc />
    public partial class newpropANalysys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WasSeen",
                table: "AIAnalysys",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WasSeen",
                table: "AIAnalysys");
        }
    }
}
