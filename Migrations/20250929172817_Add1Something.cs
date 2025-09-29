using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Diplom.Migrations
{
    /// <inheritdoc />
    public partial class Add1Something : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Penalties_Reservation_ReservationId",
                table: "Penalties");

            migrationBuilder.DropIndex(
                name: "IX_Penalties_ReservationId",
                table: "Penalties");

            migrationBuilder.DropColumn(
                name: "PenaltyId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "Penalties");

            migrationBuilder.AlterColumn<string>(
                name: "BookTitle",
                table: "Penalties",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "ReservationPenalties",
                columns: table => new
                {
                    ReservId = table.Column<int>(type: "integer", nullable: false),
                    PenalId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationPenalties", x => new { x.ReservId, x.PenalId });
                    table.ForeignKey(
                        name: "FK_ReservationPenalties_Penalties_PenalId",
                        column: x => x.PenalId,
                        principalTable: "Penalties",
                        principalColumn: "Penaltiesid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservationPenalties_Reservation_ReservId",
                        column: x => x.ReservId,
                        principalTable: "Reservation",
                        principalColumn: "Reverid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationPenalties_PenalId",
                table: "ReservationPenalties",
                column: "PenalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationPenalties");

            migrationBuilder.AddColumn<int>(
                name: "PenaltyId",
                table: "Reservation",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BookTitle",
                table: "Penalties",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservationId",
                table: "Penalties",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Penalties_ReservationId",
                table: "Penalties",
                column: "ReservationId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Penalties_Reservation_ReservationId",
                table: "Penalties",
                column: "ReservationId",
                principalTable: "Reservation",
                principalColumn: "Reverid",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
