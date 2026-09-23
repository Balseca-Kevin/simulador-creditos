using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CreditService.Migrations
{
    /// <inheritdoc />
    public partial class CatalogoAmpliadoSegmentosBCE : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "tipos_credito",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 1,
                column: "Categoria",
                value: "Consumo");

            migrationBuilder.UpdateData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 2,
                column: "Categoria",
                value: "Vivienda");

            migrationBuilder.UpdateData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 3,
                column: "Categoria",
                value: "Microcrédito");

            migrationBuilder.InsertData(
                table: "tipos_credito",
                columns: new[] { "Id", "Activo", "Categoria", "Codigo", "Descripcion", "Nombre", "TasaAnual", "TasaSeguroDesgravamenMensual" },
                values: new object[,]
                {
                    { 4, true, "Productivo", "PRODUCTIVO_CORPORATIVO", "Empresas con ventas anuales superiores a cinco millones de dólares.", "Productivo Corporativo", 6.79m, 0.0300m },
                    { 5, true, "Productivo", "PRODUCTIVO_EMPRESARIAL", "Empresas con ventas anuales entre uno y cinco millones de dólares.", "Productivo Empresarial", 8.62m, 0.0350m },
                    { 6, true, "Productivo", "PRODUCTIVO_PYMES", "Pequeñas y medianas empresas con ventas anuales de hasta un millón de dólares.", "Productivo PYMES", 9.18m, 0.0450m },
                    { 7, true, "Educativo", "EDUCATIVO", "Financiamiento de estudios de grado, posgrado y formación profesional.", "Crédito Educativo", 8.95m, 0.0450m },
                    { 8, true, "Educativo", "EDUCATIVO_SOCIAL", "Estudios para personas en situación de vulnerabilidad, con tasa preferente.", "Crédito Educativo Social", 5.49m, 0.0400m },
                    { 9, true, "Vivienda", "VIVIENDA_INTERES_SOCIAL", "Primera vivienda para familias de bajos ingresos, con tope de precio regulado.", "Vivienda de Interés Social", 4.99m, 0.0400m },
                    { 10, true, "Vivienda", "VIVIENDA_INTERES_PUBLICO", "Primera vivienda dentro de proyectos calificados por el Estado.", "Vivienda de Interés Público", 4.99m, 0.0400m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "tipos_credito");
        }
    }
}
