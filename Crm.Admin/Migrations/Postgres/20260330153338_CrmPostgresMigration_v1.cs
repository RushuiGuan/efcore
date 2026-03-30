using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Admin.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class CrmPostgresMigration_v1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "crm");

            migrationBuilder.CreateTable(
                name: "company",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "text", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "contact",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    companyid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", unicode: false, maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact", x => x.id);
                    table.ForeignKey(
                        name: "fk_contact_company_companyid",
                        column: x => x.companyid,
                        principalSchema: "crm",
                        principalTable: "company",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "address",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    contactid = table.Column<Guid>(type: "uuid", nullable: false),
                    line1 = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    line2 = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    city = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    state = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true),
                    postalcode = table.Column<string>(type: "character varying(512)", unicode: false, maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_address", x => x.id);
                    table.ForeignKey(
                        name: "fk_address_contact_contactid",
                        column: x => x.contactid,
                        principalSchema: "crm",
                        principalTable: "contact",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_address_contactid",
                schema: "crm",
                table: "address",
                column: "contactid");

            migrationBuilder.CreateIndex(
                name: "ix_company_name",
                schema: "crm",
                table: "company",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_contact_companyid",
                schema: "crm",
                table: "contact",
                column: "companyid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "address",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "contact",
                schema: "crm");

            migrationBuilder.DropTable(
                name: "company",
                schema: "crm");
        }
    }
}
