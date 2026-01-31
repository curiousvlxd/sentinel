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
    [Migration("20260130201704_InitialGroundDb")]
    partial class InitialGroundDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Sentinel.Core.Entities.Decision", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("BucketStart")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("bucket_start");

                    b.Property<DateTime>("CreatedAt")
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

                    b.HasKey("Id")
                        .HasName("pk_decisions");

                    b.HasIndex("SatelliteId", "BucketStart")
                        .HasDatabaseName("ix_decisions_satellite_id_bucket_start");

                    b.ToTable("decisions", (string)null);
                });

            modelBuilder.Entity("Sentinel.Core.Entities.Mission", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
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

                    b.HasKey("Id")
                        .HasName("pk_missions");

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

                    b.Property<DateTime>("BucketStart")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("bucket_start");

                    b.Property<double>("Confidence")
                        .HasColumnType("double precision")
                        .HasColumnName("confidence");

                    b.Property<DateTime>("CreatedAt")
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

                    b.HasKey("Id")
                        .HasName("pk_ml_health_results");

                    b.HasIndex("SatelliteId", "BucketStart")
                        .HasDatabaseName("ix_ml_health_results_satellite_id_bucket_start");

                    b.ToTable("ml_health_results", (string)null);
                });

            modelBuilder.Entity("Sentinel.Core.Entities.Command", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime?>("ClaimedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("claimed_at");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<DateTime?>("ExecutedAt")
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

                    b.HasKey("Id")
                        .HasName("pk_commands");

                    b.HasIndex("SatelliteId")
                        .HasDatabaseName("ix_commands_satellite_id");

                    b.HasIndex("SatelliteId", "Status")
                        .HasDatabaseName("ix_commands_satellite_id_status");

                    b.ToTable("commands", (string)null);
                });

            modelBuilder.Entity("Sentinel.Core.Entities.Satellite", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("id");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_at");

                    b.Property<string>("ExternalId")
                        .HasColumnType("text")
                        .HasColumnName("external_id");

                    b.Property<DateTime?>("LastBucketStart")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_bucket_start");

                    b.Property<string>("LinkStatus")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("link_status");

                    b.Property<string>("Mode")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("operating_mode");

                    b.Property<Guid?>("MissionId")
                        .HasColumnType("uuid")
                        .HasColumnName("mission_id");

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

                    b.HasKey("Id")
                        .HasName("pk_satellites");

                    b.HasIndex("MissionId")
                        .HasDatabaseName("ix_satellites_mission_id");

                    b.HasIndex("Status")
                        .HasDatabaseName("ix_satellites_status");

                    b.ToTable("satellites", (string)null);
                });

            modelBuilder.Entity("Sentinel.Core.Entities.Satellite", b =>
                {
                    b.HasOne("Sentinel.Core.Entities.Mission", null)
                        .WithMany()
                        .HasForeignKey("MissionId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .HasConstraintName("fk_satellites_missions_mission_id");
                });
#pragma warning restore 612, 618
        }
    }
}
