using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261121140000_TiendaOfertasFicha")]
    public partial class TiendaOfertasFicha : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnTienda",
                table: "Ofertas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Productos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioLista",
                table: "LineasPedidoTienda",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("UPDATE \"LineasPedidoTienda\" SET \"PrecioLista\" = \"PrecioUnitario\";");

            migrationBuilder.AddColumn<string>(
                name: "Oferta",
                table: "LineasPedidoTienda",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Ahorro",
                table: "LineasPedidoTienda",
                type: "numeric(14,2)",
                precision: 14,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Ahorro", table: "LineasPedidoTienda");
            migrationBuilder.DropColumn(name: "Oferta", table: "LineasPedidoTienda");
            migrationBuilder.DropColumn(name: "PrecioLista", table: "LineasPedidoTienda");
            migrationBuilder.DropColumn(name: "Descripcion", table: "Productos");
            migrationBuilder.DropColumn(name: "EnTienda", table: "Ofertas");
        }
    }
}
