using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIV.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnicidadVueloPorAerolineaYFecha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vuelos_Numero",
                table: "Vuelos");

            migrationBuilder.CreateIndex(
                name: "IX_Vuelos_Numero_AerolineaId",
                table: "Vuelos",
                columns: new[] { "Numero", "AerolineaId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vuelos_Numero_AerolineaId",
                table: "Vuelos");

            migrationBuilder.CreateIndex(
                name: "IX_Vuelos_Numero",
                table: "Vuelos",
                column: "Numero",
                unique: true);
        }
    }
}
