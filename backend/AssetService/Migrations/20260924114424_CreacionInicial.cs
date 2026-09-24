using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AssetService.Migrations
{
    /// <inheritdoc />
    public partial class CreacionInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias_activo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Habilitada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_activo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "activos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ValorEstimado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaAdquisicion = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_activos_categorias_activo_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "categorias_activo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "categorias_activo",
                columns: new[] { "Id", "Codigo", "Descripcion", "Habilitada", "Nombre" },
                values: new object[,]
                {
                    { 1, "VEHICULO", "Automóviles, motocicletas y vehículos de carga.", true, "Vehículo" },
                    { 2, "INMUEBLE", "Casas, departamentos, terrenos y locales comerciales.", true, "Inmueble" },
                    { 3, "MAQUINARIA", "Equipos productivos, herramientas y maquinaria industrial.", true, "Maquinaria y equipo" },
                    { 4, "INVERSION", "Depósitos a plazo, acciones y participaciones.", true, "Inversión financiera" },
                    { 5, "OTRO", "Bienes que no encajan en las categorías anteriores.", true, "Otros bienes" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_activos_CategoriaId",
                table: "activos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_activos_UsuarioId_FechaRegistro",
                table: "activos",
                columns: new[] { "UsuarioId", "FechaRegistro" });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_activo_Codigo",
                table: "categorias_activo",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activos");

            migrationBuilder.DropTable(
                name: "categorias_activo");
        }
    }
}
