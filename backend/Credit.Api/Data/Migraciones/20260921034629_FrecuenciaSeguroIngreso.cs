using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Credit.Api.Data.Migraciones
{
    /// <inheritdoc />
    public partial class FrecuenciaSeguroIngreso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TasaSeguroDesgravamenMensual",
                table: "tipos_credito",
                type: "numeric(7,4)",
                precision: 7,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FrecuenciaPago",
                table: "simulaciones",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Mensual");

            migrationBuilder.AddColumn<bool>(
                name: "IncluyeSeguroDesgravamen",
                table: "simulaciones",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "IngresoMinimoRequerido",
                table: "simulaciones",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Las simulaciones previas a esta migración eran francés mensual sin seguro:
            // su cuota más alta es la cuota fija, así que el ingreso mínimo se puede
            // reconstruir exactamente con la misma regla del motor (40 %, redondeo hacia arriba).
            migrationBuilder.Sql(
                """UPDATE simulaciones SET "IngresoMinimoRequerido" = CEIL("CuotaFija" / 0.40 * 100) / 100 WHERE "IngresoMinimoRequerido" = 0;""");

            migrationBuilder.UpdateData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 1,
                column: "TasaSeguroDesgravamenMensual",
                value: 0.0500m);

            migrationBuilder.UpdateData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 2,
                column: "TasaSeguroDesgravamenMensual",
                value: 0.0400m);

            migrationBuilder.UpdateData(
                table: "tipos_credito",
                keyColumn: "Id",
                keyValue: 3,
                column: "TasaSeguroDesgravamenMensual",
                value: 0.0700m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TasaSeguroDesgravamenMensual",
                table: "tipos_credito");

            migrationBuilder.DropColumn(
                name: "FrecuenciaPago",
                table: "simulaciones");

            migrationBuilder.DropColumn(
                name: "IncluyeSeguroDesgravamen",
                table: "simulaciones");

            migrationBuilder.DropColumn(
                name: "IngresoMinimoRequerido",
                table: "simulaciones");
        }
    }
}
