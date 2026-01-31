using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sentinel.Infrastructure.Migrations;

public partial class InitialSchema : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "missions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                description = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
            },
            constraints: table => { table.PrimaryKey("pk_missions", x => x.id); });

        migrationBuilder.CreateTable(
            name: "satellites",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                mission_id = table.Column<Guid>(type: "uuid", nullable: true),
                name = table.Column<string>(type: "text", nullable: false),
                norad_id = table.Column<int>(type: "integer", nullable: true),
                external_id = table.Column<string>(type: "text", nullable: true),
                status = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                operating_mode = table.Column<string>(type: "text", nullable: false, defaultValue: "Assisted"),
                state = table.Column<string>(type: "text", nullable: false, defaultValue: "Ok"),
                link_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Offline"),
                last_bucket_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_satellites", x => x.id);
                table.ForeignKey(
                    name: "fk_satellites_missions_mission_id",
                    column: x => x.mission_id,
                    principalTable: "missions",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "decisions",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                satellite_id = table.Column<Guid>(type: "uuid", nullable: false),
                bucket_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                decision_type = table.Column<string>(type: "text", nullable: false),
                reason = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                metadata = table.Column<string>(type: "jsonb", nullable: true),
            },
            constraints: table => { table.PrimaryKey("pk_decisions", x => x.id); });

        migrationBuilder.CreateTable(
            name: "ml_health_results",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                satellite_id = table.Column<Guid>(type: "uuid", nullable: false),
                bucket_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                model_name = table.Column<string>(type: "text", nullable: false),
                model_version = table.Column<string>(type: "text", nullable: false),
                anomaly_score = table.Column<double>(type: "double precision", nullable: false),
                confidence = table.Column<double>(type: "double precision", nullable: false),
                per_signal_score = table.Column<string>(type: "jsonb", nullable: false),
                top_contributors = table.Column<string>(type: "jsonb", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table => { table.PrimaryKey("pk_ml_health_results", x => x.id); });

        migrationBuilder.CreateTable(
            name: "command_templates",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "text", nullable: false),
                description = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
            },
            constraints: table => { table.PrimaryKey("pk_command_templates", x => x.id); });

        migrationBuilder.CreateTable(
            name: "commands",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                satellite_id = table.Column<Guid>(type: "uuid", nullable: false),
                mission_id = table.Column<Guid>(type: "uuid", nullable: true),
                command_template_id = table.Column<Guid>(type: "uuid", nullable: true),
                command_type = table.Column<string>(type: "text", nullable: false),
                payload_json = table.Column<string>(type: "text", nullable: true),
                priority = table.Column<int>(type: "integer", nullable: false),
                ttl_sec = table.Column<int>(type: "integer", nullable: false),
                status = table.Column<string>(type: "text", nullable: false),
                created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                claimed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_commands", x => x.id);
                table.ForeignKey(
                    name: "fk_commands_command_templates_command_template_id",
                    column: x => x.command_template_id,
                    principalTable: "command_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "command_template_fields",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                command_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "text", nullable: false),
                field_type = table.Column<string>(type: "text", nullable: false),
                unit = table.Column<short>(type: "smallint", nullable: false),
                default_value = table.Column<string>(type: "text", nullable: true),
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_command_template_fields", x => x.id);
                table.ForeignKey(
                    name: "fk_command_template_fields_command_templates_command_template_",
                    column: x => x.command_template_id,
                    principalTable: "command_templates",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "ix_decisions_satellite_id_bucket_start", table: "decisions", columns: new[] { "satellite_id", "bucket_start" });
        migrationBuilder.CreateIndex(name: "ix_ml_health_results_satellite_id_bucket_start", table: "ml_health_results", columns: new[] { "satellite_id", "bucket_start" });
        migrationBuilder.CreateIndex(name: "ix_satellites_mission_id", table: "satellites", column: "mission_id");
        migrationBuilder.CreateIndex(name: "ix_satellites_status", table: "satellites", column: "status");
        migrationBuilder.CreateIndex(name: "ix_commands_command_template_id", table: "commands", column: "command_template_id");
        migrationBuilder.CreateIndex(name: "ix_commands_satellite_id", table: "commands", column: "satellite_id");
        migrationBuilder.CreateIndex(name: "ix_commands_satellite_id_status", table: "commands", columns: new[] { "satellite_id", "status" });
        migrationBuilder.CreateIndex(name: "ix_command_template_fields_command_template_id", table: "command_template_fields", column: "command_template_id");
        migrationBuilder.CreateIndex(name: "ix_command_templates_type", table: "command_templates", column: "type", unique: true);

        var reducePowerId = new Guid("a1000001-0000-0000-0000-000000000001");
        var switchModeId = new Guid("a1000002-0000-0000-0000-000000000002");
        var throttlePayloadId = new Guid("a1000003-0000-0000-0000-000000000003");
        var utcSeed = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        migrationBuilder.InsertData(
            table: "command_templates",
            columns: new[] { "id", "type", "description", "created_at" },
            values: new object[,]
            {
                { reducePowerId, "ReducePower", "Reduce power consumption", utcSeed },
                { switchModeId, "SwitchMode", "Switch satellite operating mode", utcSeed },
                { throttlePayloadId, "ThrottlePayload", "Throttle payload data rate", utcSeed },
            });
        migrationBuilder.InsertData(
            table: "command_template_fields",
            columns: new[] { "id", "command_template_id", "name", "field_type", "unit", "default_value" },
            values: new object[,]
            {
                { new Guid("b1000001-0001-0000-0000-000000000001"), reducePowerId, "reduction_percent", "number", (short)1, "10" },
                { new Guid("b1000002-0001-0000-0000-000000000002"), switchModeId, "mode", "string", (short)6, "Assisted" },
                { new Guid("b1000003-0001-0000-0000-000000000003"), throttlePayloadId, "factor_percent", "number", (short)1, "80" },
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "command_template_fields");
        migrationBuilder.DropTable(name: "commands");
        migrationBuilder.DropTable(name: "command_templates");
        migrationBuilder.DropTable(name: "decisions");
        migrationBuilder.DropTable(name: "ml_health_results");
        migrationBuilder.DropTable(name: "satellites");
        migrationBuilder.DropTable(name: "missions");
    }
}
