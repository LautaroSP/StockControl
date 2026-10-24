using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261023170000_CuentasPendientes")]
    public partial class CuentasPendientes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuentasPendientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdCliente = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    NroCaja = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: true),
                    NombreUsuario = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    GananciaEstimada = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaIncobrable = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UsuarioIncobrable = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasPendientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasPendientes_Clientes_IdCliente",
                        column: x => x.IdCliente,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LineasCuenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdCuentaPendiente = table.Column<int>(type: "integer", nullable: false),
                    IdProducto = table.Column<int>(type: "integer", nullable: true),
                    Codigo = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cantidad = table.Column<decimal>(type: "numeric(14,3)", precision: 14, scale: 3, nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    Costo = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    SubTotal = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    DescripcionOferta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineasCuenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LineasCuenta_CuentasPendientes_IdCuentaPendiente",
                        column: x => x.IdCuentaPendiente,
                        principalTable: "CuentasPendientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagosCuenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdCuentaPendiente = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    IdInformeVenta = table.Column<int>(type: "integer", nullable: true),
                    IdUsuario = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosCuenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosCuenta_CuentasPendientes_IdCuentaPendiente",
                        column: x => x.IdCuentaPendiente,
                        principalTable: "CuentasPendientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_IdLocal_Nombre",
                table: "Clientes",
                columns: new[] { "IdLocal", "Nombre" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPendientes_IdCliente",
                table: "CuentasPendientes",
                column: "IdCliente");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPendientes_IdLocal_Estado_IdCliente",
                table: "CuentasPendientes",
                columns: new[] { "IdLocal", "Estado", "IdCliente" });

            migrationBuilder.CreateIndex(
                name: "IX_CuentasPendientes_IdLocal_Fecha",
                table: "CuentasPendientes",
                columns: new[] { "IdLocal", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_LineasCuenta_IdCuentaPendiente",
                table: "LineasCuenta",
                column: "IdCuentaPendiente");

            migrationBuilder.CreateIndex(
                name: "IX_PagosCuenta_IdCuentaPendiente",
                table: "PagosCuenta",
                column: "IdCuentaPendiente");

            migrationBuilder.CreateIndex(
                name: "IX_PagosCuenta_IdInformeVenta",
                table: "PagosCuenta",
                column: "IdInformeVenta");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PagosCuenta");
            migrationBuilder.DropTable(name: "LineasCuenta");
            migrationBuilder.DropTable(name: "CuentasPendientes");
            migrationBuilder.DropTable(name: "Clientes");
        }
    }
}
