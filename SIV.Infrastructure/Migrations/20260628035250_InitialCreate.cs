using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIV.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aerolineas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activa = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aerolineas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Aeropuertos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Pais = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aeropuertos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vuelos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AerolineaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AeropuertoOrigenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AeropuertoDestinoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HorarioSalida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HorarioLlegada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Puerta = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    EstadoActual = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vuelos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VueloCambiosOperativos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VueloId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ValorAnterior = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValorNuevo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegistradoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VueloCambiosOperativos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VueloCambiosOperativos_Vuelos_VueloId",
                        column: x => x.VueloId,
                        principalTable: "Vuelos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VueloHistorialEstados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VueloId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstadoAnterior = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstadoNuevo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OcurridoEn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VueloHistorialEstados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VueloHistorialEstados_Vuelos_VueloId",
                        column: x => x.VueloId,
                        principalTable: "Vuelos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aerolineas_Codigo",
                table: "Aerolineas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Aeropuertos_Codigo",
                table: "Aeropuertos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VueloCambiosOperativos_VueloId",
                table: "VueloCambiosOperativos",
                column: "VueloId");

            migrationBuilder.CreateIndex(
                name: "IX_VueloHistorialEstados_VueloId",
                table: "VueloHistorialEstados",
                column: "VueloId");

            migrationBuilder.CreateIndex(
                name: "IX_Vuelos_Numero",
                table: "Vuelos",
                column: "Numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aerolineas");

            migrationBuilder.DropTable(
                name: "Aeropuertos");

            migrationBuilder.DropTable(
                name: "VueloCambiosOperativos");

            migrationBuilder.DropTable(
                name: "VueloHistorialEstados");

            migrationBuilder.DropTable(
                name: "Vuelos");
        }
    }
}
