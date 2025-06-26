using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSOAuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixUserClientAppReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_clients_open_iddict_entity_framework_core_application_string_app_id",
                schema: "trans",
                table: "user_clients");

            migrationBuilder.DropTable(
                name: "open_iddict_entity_framework_core_token_string",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "open_iddict_entity_framework_core_authorization_string",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "open_iddict_entity_framework_core_application_string",
                schema: "trans");

            migrationBuilder.DropIndex(
                name: "ix_open_iddict_applications_client_id",
                schema: "trans",
                table: "OpenIddictApplications");

            migrationBuilder.AlterColumn<string>(
                name: "app_id",
                schema: "trans",
                table: "user_clients",
                type: "nvarchar(100)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "client_id",
                schema: "trans",
                table: "OpenIddictApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "ak_open_iddict_applications_client_id",
                schema: "trans",
                table: "OpenIddictApplications",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_applications_client_id",
                schema: "trans",
                table: "OpenIddictApplications",
                column: "client_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_user_clients_open_iddict_applications_app_id",
                schema: "trans",
                table: "user_clients",
                column: "app_id",
                principalSchema: "trans",
                principalTable: "OpenIddictApplications",
                principalColumn: "client_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_clients_open_iddict_applications_app_id",
                schema: "trans",
                table: "user_clients");

            migrationBuilder.DropUniqueConstraint(
                name: "ak_open_iddict_applications_client_id",
                schema: "trans",
                table: "OpenIddictApplications");

            migrationBuilder.DropIndex(
                name: "ix_open_iddict_applications_client_id",
                schema: "trans",
                table: "OpenIddictApplications");

            migrationBuilder.AlterColumn<string>(
                name: "app_id",
                schema: "trans",
                table: "user_clients",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)");

            migrationBuilder.AlterColumn<string>(
                name: "client_id",
                schema: "trans",
                table: "OpenIddictApplications",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.CreateTable(
                name: "open_iddict_entity_framework_core_application_string",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    application_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    client_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    client_secret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    client_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    concurrency_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    consent_type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    display_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    display_names = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    json_web_key_set = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    permissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    post_logout_redirect_uris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    redirect_uris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    settings = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_entity_framework_core_application_string", x => x.id);
                    table.UniqueConstraint("ak_open_iddict_entity_framework_core_application_string_client_id", x => x.client_id);
                });

            migrationBuilder.CreateTable(
                name: "open_iddict_entity_framework_core_authorization_string",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    application_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    concurrency_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    creation_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_entity_framework_core_authorization_string", x => x.id);
                    table.ForeignKey(
                        name: "fk_open_iddict_entity_framework_core_authorization_string_open_iddict_entity_framework_core_application_string_application_id",
                        column: x => x.application_id,
                        principalSchema: "trans",
                        principalTable: "open_iddict_entity_framework_core_application_string",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "open_iddict_entity_framework_core_token_string",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    application_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    authorization_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    concurrency_token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    creation_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expiration_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    redemption_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reference_id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_entity_framework_core_token_string", x => x.id);
                    table.ForeignKey(
                        name: "fk_open_iddict_entity_framework_core_token_string_open_iddict_entity_framework_core_application_string_application_id",
                        column: x => x.application_id,
                        principalSchema: "trans",
                        principalTable: "open_iddict_entity_framework_core_application_string",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_open_iddict_entity_framework_core_token_string_open_iddict_entity_framework_core_authorization_string_authorization_id",
                        column: x => x.authorization_id,
                        principalSchema: "trans",
                        principalTable: "open_iddict_entity_framework_core_authorization_string",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_applications_client_id",
                schema: "trans",
                table: "OpenIddictApplications",
                column: "client_id",
                unique: true,
                filter: "[client_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_entity_framework_core_authorization_string_application_id",
                schema: "trans",
                table: "open_iddict_entity_framework_core_authorization_string",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_entity_framework_core_token_string_application_id",
                schema: "trans",
                table: "open_iddict_entity_framework_core_token_string",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_entity_framework_core_token_string_authorization_id",
                schema: "trans",
                table: "open_iddict_entity_framework_core_token_string",
                column: "authorization_id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_clients_open_iddict_entity_framework_core_application_string_app_id",
                schema: "trans",
                table: "user_clients",
                column: "app_id",
                principalSchema: "trans",
                principalTable: "open_iddict_entity_framework_core_application_string",
                principalColumn: "client_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
