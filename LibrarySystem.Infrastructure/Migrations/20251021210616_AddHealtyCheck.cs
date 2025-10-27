using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddHealtyCheck : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {

        ArgumentNullException.ThrowIfNull(migrationBuilder);

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

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

        ArgumentNullException.ThrowIfNull(migrationBuilder);

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
}
