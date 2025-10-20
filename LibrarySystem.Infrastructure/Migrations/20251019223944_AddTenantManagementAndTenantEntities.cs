#pragma warning disable CA1861 // Avoid constant arrays as arguments
using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LibrarySystem.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddTenantManagementAndTenantEntities : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                SubscriptionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                SubscriptionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                MaxLibraries = table.Column<int>(type: "integer", nullable: true),
                MaxUsers = table.Column<int>(type: "integer", nullable: true),
                UseSeparateDatabase = table.Column<bool>(type: "boolean", nullable: false),
                ConnectionString = table.Column<string>(type: "text", nullable: true),
                OrganizationUnitId = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tenants", x => x.Id);
                table.ForeignKey(
                    name: "FK_Tenants_OrganizationUnits_OrganizationUnitId",
                    column: x => x.OrganizationUnitId,
                    principalTable: "OrganizationUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "TenantFeatures",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                TenantId = table.Column<int>(type: "integer", nullable: false),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TenantFeatures", x => x.Id);
                table.ForeignKey(
                    name: "FK_TenantFeatures_Tenants_TenantId",
                    column: x => x.TenantId,
                    principalTable: "Tenants",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_TenantFeatures_TenantId_Name",
            table: "TenantFeatures",
            columns: new[] { "TenantId", "Name" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Tenants_Code",
            table: "Tenants",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Tenants_OrganizationUnitId",
            table: "Tenants",
            column: "OrganizationUnitId",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropTable(
            name: "TenantFeatures");

        migrationBuilder.DropTable(
            name: "Tenants");
    }
}