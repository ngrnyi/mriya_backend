using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessengerBackend.Migrations
{
    /// <inheritdoc />
    public partial class lastchekedupdate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastProcessedMessageId",
                table: "Chats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastProcessedMessageId",
                table: "Chats");
        }
    }
}
