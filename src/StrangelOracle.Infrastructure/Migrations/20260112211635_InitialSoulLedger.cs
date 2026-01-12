using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StrangelOracle.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSoulLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "soul_ledger",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    session_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    strangel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    petition = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    response = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    outcome = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    intensity = table.Column<double>(type: "double precision", nullable: false),
                    bestowed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_soul_ledger", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_soul_ledger_session",
                table: "soul_ledger",
                column: "session_id");

            migrationBuilder.CreateIndex(
                name: "ix_soul_ledger_strangel",
                table: "soul_ledger",
                column: "strangel");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "soul_ledger");
        }
    }
}
