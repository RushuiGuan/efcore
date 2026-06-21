using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Crm.Admin.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class CrmPostgresMigration_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_address_contact_contactid",
                schema: "crm",
                table: "address");

            migrationBuilder.AlterColumn<string>(
                name: "line1",
                schema: "crm",
                table: "address",
                type: "character varying(512)",
                unicode: false,
                maxLength: 512,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldUnicode: false,
                oldMaxLength: 512,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_address_contact_contactid",
                schema: "crm",
                table: "address",
                column: "contactid",
                principalSchema: "crm",
                principalTable: "contact",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_address_contact_contactid",
                schema: "crm",
                table: "address");

            migrationBuilder.AlterColumn<string>(
                name: "line1",
                schema: "crm",
                table: "address",
                type: "character varying(512)",
                unicode: false,
                maxLength: 512,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(512)",
                oldUnicode: false,
                oldMaxLength: 512);

            migrationBuilder.AddForeignKey(
                name: "fk_address_contact_contactid",
                schema: "crm",
                table: "address",
                column: "contactid",
                principalSchema: "crm",
                principalTable: "contact",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
