using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderProjection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProjectedAt",
                table: "OrderProjections",
                newName: "LastProjectedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstProjectedAt",
                table: "OrderProjections",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstProjectedAt",
                table: "OrderProjections");

            migrationBuilder.RenameColumn(
                name: "LastProjectedAt",
                table: "OrderProjections",
                newName: "ProjectedAt");
        }
    }
}
