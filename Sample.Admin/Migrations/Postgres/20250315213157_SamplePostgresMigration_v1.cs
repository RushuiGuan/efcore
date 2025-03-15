using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sample.Admin.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class SamplePostgresMigration_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sam");

            migrationBuilder.CreateTable(
                name: "Contact",
                schema: "sam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", unicode: false, maxLength: 128, nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Property = table.Column<string>(type: "text", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                schema: "sam",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Line1 = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    Line2 = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    City = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    State = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    PostalCode = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    ContactId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Address_Contact_ContactId",
                        column: x => x.ContactId,
                        principalSchema: "sam",
                        principalTable: "Contact",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_ContactId",
                schema: "sam",
                table: "Address",
                column: "ContactId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Address",
                schema: "sam");

            migrationBuilder.DropTable(
                name: "Contact",
                schema: "sam");
        }
    }
}
