﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MessengerBackend.Migrations
{
    /// <inheritdoc />
    public partial class ischeckedadded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "wasChecked",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "wasChecked",
                table: "Messages");
        }
    }
}
