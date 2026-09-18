using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Credit.Api.Data.Migraciones
{
    /// <inheritdoc />
    public partial class CreacionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tipos_credito",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TasaAnual = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_credito", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "simulaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipoCreditoId = table.Column<int>(type: "integer", nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PlazoMeses = table.Column<int>(type: "integer", nullable: false),
                    TasaAnualAplicada = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    CuotaFija = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalInteresFrances = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalInteresAleman = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaSimulacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_simulaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_simulaciones_tipos_credito_TipoCreditoId",
                        column: x => x.TipoCreditoId,
                        principalTable: "tipos_credito",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "tipos_credito",
                columns: new[] { "Id", "Activo", "Codigo", "Descripcion", "Nombre", "TasaAnual" },
                values: new object[,]
                {
                    { 1, true, "CONSUMO", "Adquisición de bienes de consumo o pago de servicios.", "Crédito de Consumo", 15.50m },
                    { 2, true, "INMOBILIARIO", "Compra, construcción o remodelación de vivienda.", "Crédito Inmobiliario", 8.50m },
                    { 3, true, "MICROCREDITO", "Financiamiento para actividades productivas a pequeña escala.", "Microcrédito", 22.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_simulaciones_TipoCreditoId",
                table: "simulaciones",
                column: "TipoCreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_simulaciones_UsuarioId_FechaSimulacion",
                table: "simulaciones",
                columns: new[] { "UsuarioId", "FechaSimulacion" });

            migrationBuilder.CreateIndex(
                name: "IX_tipos_credito_Codigo",
                table: "tipos_credito",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "simulaciones");

            migrationBuilder.DropTable(
                name: "tipos_credito");
        }
    }
}
