using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace StockControl.Api.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class PanelPedidosTienda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Actualizado",
                table: "PedidosTienda",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Entregado",
                table: "PedidosTienda",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdInformeVenta",
                table: "PedidosTienda",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Avisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IdPedidoTienda = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Creado = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Leido = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avisos_PedidosTienda_IdPedidoTienda",
                        column: x => x.IdPedidoTienda,
                        principalTable: "PedidosTienda",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreferenciasAviso",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    Sonido = table.Column<bool>(type: "boolean", nullable: false),
                    PushNuevos = table.Column<bool>(type: "boolean", nullable: false),
                    PushCancelados = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreferenciasAviso", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuscripcionesPush",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdLocal = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    P256dh = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Auth = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Creado = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuscripcionesPush", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avisos_IdLocal_IdUsuario_Id",
                table: "Avisos",
                columns: new[] { "IdLocal", "IdUsuario", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Avisos_IdPedidoTienda",
                table: "Avisos",
                column: "IdPedidoTienda");

            migrationBuilder.CreateIndex(
                name: "IX_PreferenciasAviso_IdLocal_IdUsuario",
                table: "PreferenciasAviso",
                columns: new[] { "IdLocal", "IdUsuario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuscripcionesPush_IdLocal_IdUsuario_Endpoint",
                table: "SuscripcionesPush",
                columns: new[] { "IdLocal", "IdUsuario", "Endpoint" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avisos");

            migrationBuilder.DropTable(
                name: "PreferenciasAviso");

            migrationBuilder.DropTable(
                name: "SuscripcionesPush");

            migrationBuilder.DropColumn(
                name: "Actualizado",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "Entregado",
                table: "PedidosTienda");

            migrationBuilder.DropColumn(
                name: "IdInformeVenta",
                table: "PedidosTienda");
        }
    }
}
