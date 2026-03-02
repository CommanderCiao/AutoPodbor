using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace VehicleSearchService.Migrations
{
    /// <inheritdoc />
    public partial class v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleCardVehicleSelection");

            migrationBuilder.DropTable(
                name: "VehicleCard");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Vehicles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VehicleVehicleSelection",
                columns: table => new
                {
                    VehicleSelectionId = table.Column<int>(type: "integer", nullable: false),
                    VehiclesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleVehicleSelection", x => new { x.VehicleSelectionId, x.VehiclesId });
                    table.ForeignKey(
                        name: "FK_VehicleVehicleSelection_VehicleSelections_VehicleSelectionId",
                        column: x => x.VehicleSelectionId,
                        principalTable: "VehicleSelections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleVehicleSelection_Vehicles_VehiclesId",
                        column: x => x.VehiclesId,
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleVehicleSelection_VehiclesId",
                table: "VehicleVehicleSelection",
                column: "VehiclesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleVehicleSelection");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Vehicles");

            migrationBuilder.CreateTable(
                name: "VehicleCard",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Brand = table.Column<string>(type: "text", nullable: false),
                    Kilometrage = table.Column<int>(type: "integer", nullable: false),
                    Model = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    VehicleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleCard", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VehicleCardVehicleSelection",
                columns: table => new
                {
                    VehicleSelectionId = table.Column<int>(type: "integer", nullable: false),
                    VehiclesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleCardVehicleSelection", x => new { x.VehicleSelectionId, x.VehiclesId });
                    table.ForeignKey(
                        name: "FK_VehicleCardVehicleSelection_VehicleCard_VehiclesId",
                        column: x => x.VehiclesId,
                        principalTable: "VehicleCard",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleCardVehicleSelection_VehicleSelections_VehicleSelect~",
                        column: x => x.VehicleSelectionId,
                        principalTable: "VehicleSelections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleCardVehicleSelection_VehiclesId",
                table: "VehicleCardVehicleSelection",
                column: "VehiclesId");
        }
    }
}
