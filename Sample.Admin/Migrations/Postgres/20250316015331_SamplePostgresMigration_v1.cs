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
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(128)", unicode: false, maxLength: 128, nullable: false),
                    createdutc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    property = table.Column<string>(type: "text", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                schema: "sam",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    line1 = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    line2 = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    city = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    state = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    postalcode = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    contactid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_address", x => x.id);
                    table.ForeignKey(
                        name: "fk_address_contact_contactid",
                        column: x => x.contactid,
                        principalSchema: "sam",
                        principalTable: "Contact",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_address_contactid",
                schema: "sam",
                table: "Address",
                column: "contactid");
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
