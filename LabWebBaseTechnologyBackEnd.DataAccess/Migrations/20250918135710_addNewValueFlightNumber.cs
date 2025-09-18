using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabWebBaseTechnologyBackEnd.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addNewValueFlightNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FlightNumber",
                table: "Flights",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlightNumber",
                table: "Flights");
        }
    }
}
