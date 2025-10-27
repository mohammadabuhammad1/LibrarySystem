using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSoftDelete : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<string>(
            name: "DeleteBy",
            table: "Libraries",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "DeletedAt",
            table: "Libraries",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Libraries",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "DeleteBy",
            table: "BorrowRecords",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "DeletedAt",
            table: "BorrowRecords",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "BorrowRecords",
            type: "boolean",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "DeleteBy",
            table: "Books",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "DeletedAt",
            table: "Books",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "IsDeleted",
            table: "Books",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropColumn(
            name: "DeleteBy",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "DeletedAt",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "DeleteBy",
            table: "BorrowRecords");

        migrationBuilder.DropColumn(
            name: "DeletedAt",
            table: "BorrowRecords");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "BorrowRecords");

        migrationBuilder.DropColumn(
            name: "DeleteBy",
            table: "Books");

        migrationBuilder.DropColumn(
            name: "DeletedAt",
            table: "Books");

        migrationBuilder.DropColumn(
            name: "IsDeleted",
            table: "Books");
    }
}
