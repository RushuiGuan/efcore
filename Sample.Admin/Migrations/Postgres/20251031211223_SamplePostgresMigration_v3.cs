using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sample.Admin.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class SamplePostgresMigration_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "createdby",
                schema: "sam",
                table: "contact",
                type: "text",
                unicode: false,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "modifiedby",
                schema: "sam",
                table: "contact",
                type: "text",
                unicode: false,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "modifiedutc",
                schema: "sam",
                table: "contact",
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
                table: "contact");

            migrationBuilder.DropColumn(
                name: "modifiedby",
                schema: "sam",
                table: "contact");

            migrationBuilder.DropColumn(
                name: "modifiedutc",
                schema: "sam",
                table: "contact");
        }
    }
}
