using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Crm.Admin.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class CrmPostgresMigration_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit",
                schema: "crm",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    entityid = table.Column<Guid>(type: "uuid", nullable: false),
                    actorid = table.Column<Guid>(type: "uuid", nullable: false),
                    utctimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    changetype = table.Column<int>(type: "integer", nullable: false),
                    entitytype = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    json = table.Column<string>(type: "text", unicode: false, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit",
                schema: "crm");
        }
    }
}
