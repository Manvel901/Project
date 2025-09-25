using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diplom.Migrations
{
    /// <inheritdoc />
    public partial class AddsECONdSomething : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Penalties_Revervation_ReservationId",
                table: "Penalties");

            migrationBuilder.DropIndex(
                name: "IX_Penalties_ReservationId",
                table: "Penalties");

            migrationBuilder.DropColumn(
                name: "AutorLastname",
                table: "Autors");

            migrationBuilder.DropColumn(
                name: "AutorSurname",
                table: "Autors");

            migrationBuilder.AddColumn<int>(
                name: "PenaltyId",
                table: "Revervation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Revervation_PenaltyId",
                table: "Revervation",
                column: "PenaltyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Autors_AutorName",
                table: "Autors",
                column: "AutorName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Revervation_Penalties_PenaltyId",
                table: "Revervation",
                column: "PenaltyId",
                principalTable: "Penalties",
                principalColumn: "Penaltiesid",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Revervation_Penalties_PenaltyId",
                table: "Revervation");

            migrationBuilder.DropIndex(
                name: "IX_Revervation_PenaltyId",
                table: "Revervation");

            migrationBuilder.DropIndex(
                name: "IX_Autors_AutorName",
                table: "Autors");

            migrationBuilder.DropColumn(
                name: "PenaltyId",
                table: "Revervation");

            migrationBuilder.AddColumn<string>(
                name: "AutorLastname",
                table: "Autors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AutorSurname",
                table: "Autors",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Penalties_ReservationId",
                table: "Penalties",
                column: "ReservationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Penalties_Revervation_ReservationId",
                table: "Penalties",
                column: "ReservationId",
                principalTable: "Revervation",
                principalColumn: "Reverid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
