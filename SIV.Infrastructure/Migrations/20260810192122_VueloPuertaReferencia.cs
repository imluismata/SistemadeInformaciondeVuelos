using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIV.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class VueloPuertaReferencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Puerta",
                table: "Vuelos");

            migrationBuilder.AddColumn<string>(
                name: "PuertaDescripcion",
                table: "Vuelos",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PuertaId",
                table: "Vuelos",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PuertaDescripcion",
                table: "Vuelos");

            migrationBuilder.DropColumn(
                name: "PuertaId",
                table: "Vuelos");

            migrationBuilder.AddColumn<string>(
                name: "Puerta",
                table: "Vuelos",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }
    }
}
