using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LibrarySystem.Infrastructure.Migrations;

/// <inheritdoc />
public partial class OrganizationUnit : Migration
{
    // Static readonly field for the column names to fix the analyzer warning
    private static readonly string[] UserIdOrganizationUnitIdColumns = { "UserId", "OrganizationUnitId" };

    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        // First create the OrganizationUnits table
        migrationBuilder.CreateTable(
            name: "OrganizationUnits",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Code = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                ParentId = table.Column<int>(type: "integer", nullable: true),
                IsActive = table.Column<bool>(type: "boolean", nullable: false),
                Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                SubscriptionStartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                SubscriptionEndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                MaxLibraries = table.Column<int>(type: "integer", nullable: true),
                MaxUsers = table.Column<int>(type: "integer", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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

        // Create UserOrganizationUnits table
        migrationBuilder.CreateTable(
            name: "UserOrganizationUnits",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UserId = table.Column<string>(type: "text", nullable: false),
                OrganizationUnitId = table.Column<int>(type: "integer", nullable: false),
                IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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

        // Add OrganizationUnitId as nullable first
        migrationBuilder.AddColumn<int>(
            name: "OrganizationUnitId",
            table: "Libraries",
            type: "integer",
            nullable: true,
            defaultValue: null);

        // Create indexes for OrganizationUnits
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

        // Create indexes for UserOrganizationUnits
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
            columns: UserIdOrganizationUnitIdColumns,
            unique: true);

        // Create a default organization unit
        migrationBuilder.Sql(@"
            INSERT INTO ""OrganizationUnits"" (""Code"", ""DisplayName"", ""Description"", ""IsActive"", ""Type"", ""CreatedAt"")
            VALUES ('0000', 'Default Organization', 'Default organization for existing libraries', true, 'Tenant', NOW())
        ");

        // Update existing libraries to use the default organization unit
        migrationBuilder.Sql(@"
            UPDATE ""Libraries"" 
            SET ""OrganizationUnitId"" = (SELECT ""Id"" FROM ""OrganizationUnits"" WHERE ""Code"" = '0000')
        ");

        // Now make the OrganizationUnitId column non-nullable
        migrationBuilder.AlterColumn<int>(
            name: "OrganizationUnitId",
            table: "Libraries",
            type: "integer",
            nullable: false,
            defaultValue: 1,
            oldClrType: typeof(int),
            oldType: "integer",
            oldNullable: true);

        // Create index for Libraries after the data is populated
        migrationBuilder.CreateIndex(
            name: "IX_Libraries_OrganizationUnitId",
            table: "Libraries",
            column: "OrganizationUnitId");

        // Finally add the foreign key constraint
        migrationBuilder.AddForeignKey(
            name: "FK_Libraries_OrganizationUnits_OrganizationUnitId",
            table: "Libraries",
            column: "OrganizationUnitId",
            principalTable: "OrganizationUnits",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        // Update AspNetUsers columns
        migrationBuilder.AlterColumn<string>(
            name: "Phone",
            table: "AspNetUsers",
            type: "character varying(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(20)",
            oldMaxLength: 20);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "AspNetUsers",
            type: "character varying(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropForeignKey(
            name: "FK_Libraries_OrganizationUnits_OrganizationUnitId",
            table: "Libraries");

        migrationBuilder.DropTable(
            name: "UserOrganizationUnits");

        migrationBuilder.DropTable(
            name: "OrganizationUnits");

        migrationBuilder.DropIndex(
            name: "IX_Libraries_OrganizationUnitId",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "OrganizationUnitId",
            table: "Libraries");

        migrationBuilder.AlterColumn<string>(
            name: "Phone",
            table: "AspNetUsers",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(50)",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "AspNetUsers",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(200)",
            oldMaxLength: 200);
    }
}