using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PreferredBand",
                table: "ClientRequests",
                newName: "PreferredBrand");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ClientRequests",
                newName: "ClientRequestId");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "ClientRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ClientRequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ClientRequests",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ClientRequests");

            migrationBuilder.RenameColumn(
                name: "PreferredBrand",
                table: "ClientRequests",
                newName: "PreferredBand");

            migrationBuilder.RenameColumn(
                name: "ClientRequestId",
                table: "ClientRequests",
                newName: "Id");
        }
    }
}
