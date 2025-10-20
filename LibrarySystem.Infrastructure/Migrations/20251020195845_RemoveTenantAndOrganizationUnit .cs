using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1861 

namespace LibrarySystem.Infrastructure.Migrations;

/// <inheritdoc />
public partial class RemoveTenantAndOrganizationUnit : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropForeignKey(
            name: "FK_Libraries_OrganizationUnits_OrganizationUnitId",
            table: "Libraries");

        migrationBuilder.DropTable(
            name: "TenantFeatures");

        migrationBuilder.DropTable(
            name: "UserOrganizationUnits");

        migrationBuilder.DropTable(
            name: "Tenants");

        migrationBuilder.DropTable(
            name: "OrganizationUnits");

        migrationBuilder.DropIndex(
            name: "IX_Libraries_OrganizationUnitId",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "OrganizationUnitId",
            table: "Libraries");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);


        migrationBuilder.AddColumn<int>(
            name: "OrganizationUnitId",
            table: "Libraries",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.CreateTable(
            name: "OrganizationUnits",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                ParentId = table.Column<int>(type: "integer", nullable: true),
                Code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                MaxLibraries = table.Column<int>(type: "integer", nullable: true),
                MaxUsers = table.Column<int>(type: "integer", nullable: true),
                SubscriptionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                SubscriptionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OrganizationUnits", x => x.Id);
                table.ForeignKey(
                    name: "FK_OrganizationUnits_OrganizationUnits_ParentId",
                    column: x => x.ParentId,
                    principalTable: "OrganizationUnits",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Tenants",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                OrganizationUnitId = table.Column<int>(type: "integer", nullable: false),
                Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                ConnectionString = table.Column<string>(type: "text", nullable: true),
                ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                MaxLibraries = table.Column<int>(type: "integer", nullable: true),
                MaxUsers = table.Column<int>(type: "integer", nullable: true),
                SubscriptionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                SubscriptionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                UseSeparateDatabase = table.Column<bool>(type: "boolean", nullable: false)
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
            name: "UserOrganizationUnits",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                OrganizationUnitId = table.Column<int>(type: "integer", nullable: false),
                UserId = table.Column<string>(type: "text", nullable: false),
                AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                IsDefault = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserOrganizationUnits", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserOrganizationUnits_AspNetUsers_UserId",
                    column: x => x.UserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_UserOrganizationUnits_OrganizationUnits_OrganizationUnitId",
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
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CreatedBy = table.Column<string>(type: "text", nullable: true),
                Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                UpdatedBy = table.Column<string>(type: "text", nullable: true),
                Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
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
            name: "IX_Libraries_OrganizationUnitId",
            table: "Libraries",
            column: "OrganizationUnitId");

        migrationBuilder.CreateIndex(
            name: "IX_OrganizationUnits_Code",
            table: "OrganizationUnits",
            column: "Code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_OrganizationUnits_IsActive",
            table: "OrganizationUnits",
            column: "IsActive");

        migrationBuilder.CreateIndex(
            name: "IX_OrganizationUnits_ParentId",
            table: "OrganizationUnits",
            column: "ParentId");

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

        migrationBuilder.CreateIndex(
            name: "IX_UserOrganizationUnits_IsDefault",
            table: "UserOrganizationUnits",
            column: "IsDefault");

        migrationBuilder.CreateIndex(
            name: "IX_UserOrganizationUnits_OrganizationUnitId",
            table: "UserOrganizationUnits",
            column: "OrganizationUnitId");

        migrationBuilder.CreateIndex(
            name: "IX_UserOrganizationUnits_UserId_OrganizationUnitId",
            table: "UserOrganizationUnits",
            columns: new[] { "UserId", "OrganizationUnitId" },
            unique: true);

        migrationBuilder.AddForeignKey(
            name: "FK_Libraries_OrganizationUnits_OrganizationUnitId",
            table: "Libraries",
            column: "OrganizationUnitId",
            principalTable: "OrganizationUnits",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
