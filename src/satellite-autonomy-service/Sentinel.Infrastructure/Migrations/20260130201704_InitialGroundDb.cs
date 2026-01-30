using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sentinel.Infrastructure.Migrations
{
    public partial class InitialGroundDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    satellite_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bucket_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    decision_type = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_decisions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "missions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_missions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ml_health_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    satellite_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bucket_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    model_name = table.Column<string>(type: "text", nullable: false),
                    model_version = table.Column<string>(type: "text", nullable: false),
                    anomaly_score = table.Column<double>(type: "double precision", nullable: false),
                    confidence = table.Column<double>(type: "double precision", nullable: false),
                    per_signal_score = table.Column<string>(type: "jsonb", nullable: false),
                    top_contributors = table.Column<string>(type: "jsonb", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ml_health_results", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "satellites",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    mission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    norad_id = table.Column<int>(type: "integer", nullable: true),
                    external_id = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_satellites", x => x.id);
                    table.ForeignKey(
                        name: "fk_satellites_missions_mission_id",
                        column: x => x.mission_id,
                        principalTable: "missions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_decisions_satellite_id_bucket_start",
                table: "decisions",
                columns: new[] { "satellite_id", "bucket_start" });

            migrationBuilder.CreateIndex(
                name: "ix_ml_health_results_satellite_id_bucket_start",
                table: "ml_health_results",
                columns: new[] { "satellite_id", "bucket_start" });

            migrationBuilder.CreateIndex(
                name: "ix_satellites_mission_id",
                table: "satellites",
                column: "mission_id");

            migrationBuilder.CreateIndex(
                name: "ix_satellites_status",
                table: "satellites",
                column: "status");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "decisions");

            migrationBuilder.DropTable(
                name: "ml_health_results");

            migrationBuilder.DropTable(
                name: "satellites");

            migrationBuilder.DropTable(
                name: "missions");
        }
    }
}
