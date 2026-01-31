using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Sentinel.Infrastructure.Persistence;

#nullable disable

namespace Sentinel.Infrastructure.Migrations
{
    [DbContext(typeof(GroundDbContext))]
    [Migration("20260130000000_InitialSchema")]
    partial class InitialSchema
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Sentinel.Core.Entities.SatelliteOperation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");
                    b.Property<DateTimeOffset?>("ClaimedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("claimed_at");
                    b.Property<Guid?>("CommandTemplateId")
                        .HasColumnType("uuid")
                        .HasColumnName("command_template_id");
                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");
                    b.Property<DateTimeOffset?>("ExecutedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("executed_at");
                    b.Property<Guid?>("MissionId")
                        .HasColumnType("uuid")
                        .HasColumnName("mission_id");
                    b.Property<string>("PayloadJson")
                        .HasColumnType("text")
                        .HasColumnName("payload_json");
                    b.Property<int>("Priority")
                        .HasColumnType("integer")
                        .HasColumnName("priority");
                    b.Property<Guid>("SatelliteId")
                        .HasColumnType("uuid")
                        .HasColumnName("satellite_id");
                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");
                    b.Property<int>("TtlSec")
                        .HasColumnType("integer")
                        .HasColumnName("ttl_sec");
                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("command_type");
                    b.HasKey("Id").HasName("pk_commands");
                    b.HasIndex("CommandTemplateId").HasDatabaseName("ix_commands_command_template_id");
                    b.HasIndex("SatelliteId").HasDatabaseName("ix_commands_satellite_id");
                    b.HasIndex("SatelliteId", "Status").HasDatabaseName("ix_commands_satellite_id_status");
                    b.ToTable("commands", (string)null);
                });

            modelBuilder.Entity("Sentinel.Core.Entities.CommandTemplate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");
                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");
                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("description");
                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("type");
                    b.HasKey("Id").HasName("pk_command_templates");
                    b.HasIndex("Type").IsUnique().HasDatabaseName("ix_command_templates_type");
                    b.ToTable("command_templates", (string)null);
                });

            modelBuilder.Entity("Sentinel.Core.Entities.CommandTemplateField", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");
                    b.Property<Guid>("CommandTemplateId")
                        .HasColumnType("uuid")
                        .HasColumnName("command_template_id");
                    b.Property<string>("DefaultValue")
                        .HasColumnType("text")
                        .HasColumnName("default_value");
                    b.Property<string>("FieldType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("field_type");
                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");
                    b.Property<short>("Unit")
                        .HasColumnType("smallint")
                        .HasColumnName("unit");
                    b.HasKey("Id").HasName("pk_command_template_fields");
                    b.HasIndex("CommandTemplateId").HasDatabaseName("ix_command_template_fields_command_template_id");
                    b.ToTable("command_template_fields", (string)null);
                });

            modelBuilder.Entity("Sentinel.Core.Entities.Decision", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");
                    b.Property<DateTimeOffset>("BucketStart")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("bucket_start");
                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");
                    b.Property<string>("DecisionType")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("decision_type");
                    b.Property<string>("Metadata")
                        .HasColumnType("jsonb")
                        .HasColumnName("metadata");
                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("reason");
                    b.Property<Guid>("SatelliteId")
                        .HasColumnType("uuid")
                        .HasColumnName("satellite_id");
                    b.HasKey("Id").HasName("pk_decisions");
                    b.HasIndex("SatelliteId", "BucketStart").HasDatabaseName("ix_decisions_satellite_id_bucket_start");
                    b.ToTable("decisions", (string)null);
                });

            modelBuilder.Entity("Sentinel.Core.Entities.Mission", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");
                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");
                    b.Property<string>("Description")
                        .HasColumnType("text")
                        .HasColumnName("description");
                    b.Property<bool>("IsActive")
                        .HasColumnType("boolean")
                        .HasColumnName("is_active");
                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");
                    b.HasKey("Id").HasName("pk_missions");
                    b.ToTable("missions", (string)null);
                });

            modelBuilder.Entity("Sentinel.Core.Entities.MlHealthResult", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");
                    b.Property<double>("AnomalyScore")
                        .HasColumnType("double precision")
                        .HasColumnName("anomaly_score");
                    b.Property<DateTimeOffset>("BucketStart")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("bucket_start");
                    b.Property<double>("Confidence")
                        .HasColumnType("double precision")
                        .HasColumnName("confidence");
                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");
                    b.Property<string>("ModelName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("model_name");
                    b.Property<string>("ModelVersion")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("model_version");
                    b.Property<string>("PerSignalScore")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("per_signal_score");
                    b.Property<Guid>("SatelliteId")
                        .HasColumnType("uuid")
                        .HasColumnName("satellite_id");
                    b.Property<string>("TopContributors")
                        .IsRequired()
                        .HasColumnType("jsonb")
                        .HasColumnName("top_contributors");
                    b.HasKey("Id").HasName("pk_ml_health_results");
                    b.HasIndex("SatelliteId", "BucketStart").HasDatabaseName("ix_ml_health_results_satellite_id_bucket_start");
                    b.ToTable("ml_health_results", (string)null);
                });

            modelBuilder.Entity("Sentinel.Core.Entities.Satellite", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");
                    b.Property<DateTimeOffset>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");
                    b.Property<string>("ExternalId")
                        .HasColumnType("text")
                        .HasColumnName("external_id");
                    b.Property<DateTimeOffset?>("LastBucketStart")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_bucket_start");
                    b.Property<string>("LinkStatus")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("link_status");
                    b.Property<Guid?>("MissionId")
                        .HasColumnType("uuid")
                        .HasColumnName("mission_id");
                    b.Property<string>("Mode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("operating_mode");
                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("name");
                    b.Property<int?>("NoradId")
                        .HasColumnType("integer")
                        .HasColumnName("norad_id");
                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("state");
                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("status");
                    b.HasKey("Id").HasName("pk_satellites");
                    b.HasIndex("MissionId").HasDatabaseName("ix_satellites_mission_id");
                    b.HasIndex("Status").HasDatabaseName("ix_satellites_status");
                    b.ToTable("satellites", (string)null);
                });

            modelBuilder.Entity("Sentinel.Core.Entities.SatelliteOperation", b =>
                {
                    b.HasOne("Sentinel.Core.Entities.CommandTemplate", "CommandTemplate")
                        .WithMany()
                        .HasForeignKey("CommandTemplateId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_commands_command_templates_command_template_id");
                    b.Navigation("CommandTemplate");
                });

            modelBuilder.Entity("Sentinel.Core.Entities.CommandTemplateField", b =>
                {
                    b.HasOne("Sentinel.Core.Entities.CommandTemplate", "CommandTemplate")
                        .WithMany("Fields")
                        .HasForeignKey("CommandTemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_command_template_fields_command_templates_command_template_");
                    b.Navigation("CommandTemplate");
                });

            modelBuilder.Entity("Sentinel.Core.Entities.Satellite", b =>
                {
                    b.HasOne("Sentinel.Core.Entities.Mission", null)
                        .WithMany()
                        .HasForeignKey("MissionId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_satellites_missions_mission_id");
                });

            modelBuilder.Entity("Sentinel.Core.Entities.CommandTemplate", b =>
                {
                    b.Navigation("Fields");
                });
#pragma warning restore 612, 618
        }
    }
}
