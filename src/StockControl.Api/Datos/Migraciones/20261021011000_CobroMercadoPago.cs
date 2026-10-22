using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StockControl.Api.Datos;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20261021011000_CobroMercadoPago")]
    public partial class CobroMercadoPago : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Integracion",
                table: "MetodosPago",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "manual");

            migrationBuilder.CreateTable(
                name: "CuentasMercadoPago",
                columns: table => new
                {
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    TokenAcceso = table.Column<string>(type: "text", nullable: false),
                    TokenRenovacion = table.Column<string>(type: "text", nullable: false),
                    Vence = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IdUsuarioMp = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasMercadoPago", x => x.IdLocal);
                });

            migrationBuilder.CreateTable(
                name: "PuestosMercadoPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    NroCaja = table.Column<int>(type: "integer", nullable: false),
                    IdExterno = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    IdSucursal = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    UrlQr = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IdLector = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuestosMercadoPago", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CobrosMercadoPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    NroCaja = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: true),
                    Modo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Importe = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: false),
                    IdOrdenMp = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    QrData = table.Column<string>(type: "text", nullable: false),
                    PedidoJson = table.Column<string>(type: "text", nullable: false),
                    IdInformeVenta = table.Column<int>(type: "integer", nullable: true),
                    Creado = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CobrosMercadoPago", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PuestosMercadoPago_IdLocal_NroCaja",
                table: "PuestosMercadoPago",
                columns: new[] { "IdLocal", "NroCaja" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CobrosMercadoPago_IdOrdenMp",
                table: "CobrosMercadoPago",
                column: "IdOrdenMp",
                unique: true,
                filter: "\"IdOrdenMp\" <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_CobrosMercadoPago_QrAbierto",
                table: "CobrosMercadoPago",
                columns: new[] { "IdLocal", "NroCaja" },
                unique: true,
                filter: "\"Estado\" = 'esperando' AND \"Modo\" = 'qr_impreso'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CobrosMercadoPago");
            migrationBuilder.DropTable(name: "PuestosMercadoPago");
            migrationBuilder.DropTable(name: "CuentasMercadoPago");
            migrationBuilder.DropColumn(name: "Integracion", table: "MetodosPago");
        }
    }
}
