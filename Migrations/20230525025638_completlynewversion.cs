using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessengerBackend.Migrations
{
    /// <inheritdoc />
    public partial class completlynewversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIAnalysysSeen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnalysysId = table.Column<int>(type: "int", nullable: false),
                    WasSeen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIAnalysysSeen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIAnalysysSeen_AIAnalysys_AnalysysId",
                        column: x => x.AnalysysId,
                        principalTable: "AIAnalysys",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIAnalysysSeen_AnalysysId",
                table: "AIAnalysysSeen",
                column: "AnalysysId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIAnalysysSeen");
        }
    }
}
