using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Admin.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class SampleSqlServerMigration_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "sam",
                table: "Contact",
                type: "varchar(max)",
                unicode: false,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "sam",
                table: "Contact",
                type: "varchar(max)",
                unicode: false,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedUtc",
                schema: "sam",
                table: "Contact",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "sam",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "sam",
                table: "Contact");

            migrationBuilder.DropColumn(
                name: "ModifiedUtc",
                schema: "sam",
                table: "Contact");
        }
    }
}
