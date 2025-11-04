using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Admin.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class SamplePostgresMigration_v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "modifiedby",
                schema: "sam",
                table: "contact",
                type: "character varying(128)",
                unicode: false,
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldUnicode: false);

            migrationBuilder.AlterColumn<string>(
                name: "createdby",
                schema: "sam",
                table: "contact",
                type: "character varying(128)",
                unicode: false,
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldUnicode: false);

            migrationBuilder.AddColumn<string>(
                name: "createdby",
                schema: "sam",
                table: "address",
                type: "character varying(128)",
                unicode: false,
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "createdutc",
                schema: "sam",
                table: "address",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "modifiedby",
                schema: "sam",
                table: "address",
                type: "character varying(128)",
                unicode: false,
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "modifiedutc",
                schema: "sam",
                table: "address",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "createdby",
                schema: "sam",
                table: "address");

            migrationBuilder.DropColumn(
                name: "createdutc",
                schema: "sam",
                table: "address");

            migrationBuilder.DropColumn(
                name: "modifiedby",
                schema: "sam",
                table: "address");

            migrationBuilder.DropColumn(
                name: "modifiedutc",
                schema: "sam",
                table: "address");

            migrationBuilder.AlterColumn<string>(
                name: "modifiedby",
                schema: "sam",
                table: "contact",
                type: "text",
                unicode: false,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldUnicode: false,
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "createdby",
                schema: "sam",
                table: "contact",
                type: "text",
                unicode: false,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldUnicode: false,
                oldMaxLength: 128);
        }
    }
}
