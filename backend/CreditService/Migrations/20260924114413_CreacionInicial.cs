using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CreditService.Migrations
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
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TasaAnual = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TasaSeguroDesgravamenMensual = table.Column<decimal>(type: "decimal(7,4)", precision: 7, scale: 4, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_credito", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "simulaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoCreditoId = table.Column<int>(type: "int", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlazoMeses = table.Column<int>(type: "int", nullable: false),
                    FrecuenciaPago = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IncluyeSeguroDesgravamen = table.Column<bool>(type: "bit", nullable: false),
                    TasaAnualAplicada = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CuotaFija = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalInteresFrances = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalInteresAleman = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IngresoMinimoRequerido = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaSimulacion = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                columns: new[] { "Id", "Activo", "Categoria", "Codigo", "Descripcion", "Nombre", "TasaAnual", "TasaSeguroDesgravamenMensual" },
                values: new object[,]
                {
                    { 1, true, "Consumo", "CONSUMO", "Adquisición de bienes de consumo o pago de servicios.", "Crédito de Consumo", 15.50m, 0.0500m },
                    { 2, true, "Vivienda", "INMOBILIARIO", "Compra, construcción o remodelación de vivienda.", "Crédito Inmobiliario", 8.50m, 0.0400m },
                    { 3, true, "Microcrédito", "MICROCREDITO", "Financiamiento para actividades productivas a pequeña escala.", "Microcrédito", 22.00m, 0.0700m },
                    { 4, true, "Productivo", "PRODUCTIVO_CORPORATIVO", "Empresas con ventas anuales superiores a cinco millones de dólares.", "Productivo Corporativo", 6.79m, 0.0300m },
                    { 5, true, "Productivo", "PRODUCTIVO_EMPRESARIAL", "Empresas con ventas anuales entre uno y cinco millones de dólares.", "Productivo Empresarial", 8.62m, 0.0350m },
                    { 6, true, "Productivo", "PRODUCTIVO_PYMES", "Pequeñas y medianas empresas con ventas anuales de hasta un millón de dólares.", "Productivo PYMES", 9.18m, 0.0450m },
                    { 7, true, "Educativo", "EDUCATIVO", "Financiamiento de estudios de grado, posgrado y formación profesional.", "Crédito Educativo", 8.95m, 0.0450m },
                    { 8, true, "Educativo", "EDUCATIVO_SOCIAL", "Estudios para personas en situación de vulnerabilidad, con tasa preferente.", "Crédito Educativo Social", 5.49m, 0.0400m },
                    { 9, true, "Vivienda", "VIVIENDA_INTERES_SOCIAL", "Primera vivienda para familias de bajos ingresos, con tope de precio regulado.", "Vivienda de Interés Social", 4.99m, 0.0400m },
                    { 10, true, "Vivienda", "VIVIENDA_INTERES_PUBLICO", "Primera vivienda dentro de proyectos calificados por el Estado.", "Vivienda de Interés Público", 4.99m, 0.0400m }
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
