using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddDDDPrinciple : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {

        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Libraries",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "Libraries",
            type: "text",
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "BorrowRecords",
            type: "character varying(450)",
            maxLength: 450,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "text");

        migrationBuilder.AlterColumn<string>(
            name: "Notes",
            table: "BorrowRecords",
            type: "character varying(1000)",
            maxLength: 1000,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(500)",
            oldMaxLength: 500,
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "FineAmount",
            table: "BorrowRecords",
            type: "numeric(18,2)",
            precision: 18,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(10,2)",
            oldPrecision: 10,
            oldScale: 2,
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAt",
            table: "BorrowRecords",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone",
            oldDefaultValueSql: "NOW()");

        migrationBuilder.AlterColumn<DateTime>(
            name: "BorrowDate",
            table: "BorrowRecords",
            type: "timestamp with time zone",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone",
            oldDefaultValueSql: "NOW()");

        migrationBuilder.AddColumn<string>(
            name: "Condition",
            table: "BorrowRecords",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "");

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "BorrowRecords",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "RenewalCount",
            table: "BorrowRecords",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "BorrowRecords",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "CreatedBy",
            table: "Books",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Description",
            table: "Books",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "Genre",
            table: "Books",
            type: "text",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "UpdatedBy",
            table: "Books",
            type: "text",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {

        ArgumentNullException.ThrowIfNull(migrationBuilder);

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "Condition",
            table: "BorrowRecords");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "BorrowRecords");

        migrationBuilder.DropColumn(
            name: "RenewalCount",
            table: "BorrowRecords");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "BorrowRecords");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Books");

        migrationBuilder.DropColumn(
            name: "Description",
            table: "Books");

        migrationBuilder.DropColumn(
            name: "Genre",
            table: "Books");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Books");

        migrationBuilder.AlterColumn<string>(
            name: "UserId",
            table: "BorrowRecords",
            type: "text",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(450)",
            oldMaxLength: 450);

        migrationBuilder.AlterColumn<string>(
            name: "Notes",
            table: "BorrowRecords",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(1000)",
            oldMaxLength: 1000,
            oldNullable: true);

        migrationBuilder.AlterColumn<decimal>(
            name: "FineAmount",
            table: "BorrowRecords",
            type: "numeric(10,2)",
            precision: 10,
            scale: 2,
            nullable: true,
            oldClrType: typeof(decimal),
            oldType: "numeric(18,2)",
            oldPrecision: 18,
            oldScale: 2,
            oldNullable: true);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedAt",
            table: "BorrowRecords",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "NOW()",
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");

        migrationBuilder.AlterColumn<DateTime>(
            name: "BorrowDate",
            table: "BorrowRecords",
            type: "timestamp with time zone",
            nullable: false,
            defaultValueSql: "NOW()",
            oldClrType: typeof(DateTime),
            oldType: "timestamp with time zone");
    }
}
