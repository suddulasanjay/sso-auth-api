using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SSOAuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "trans");

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
                name: "OpenIddictApplications",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    application_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    client_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    client_secret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    client_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    concurrency_token = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    consent_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("pk_open_iddict_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictScopes",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    concurrency_token = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    descriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    display_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    display_names = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    resources = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_scopes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "providers",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    client_id = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    created_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_time = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_providers", x => x.id);
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
                name: "OpenIddictAuthorizations",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    application_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    concurrency_token = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    scopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_authorizations", x => x.id);
                    table.ForeignKey(
                        name: "fk_open_iddict_authorizations_open_iddict_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "trans",
                        principalTable: "OpenIddictApplications",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    first_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password_modified_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    verified = table.Column<bool>(type: "bit", nullable: false),
                    is_admin = table.Column<bool>(type: "bit", nullable: false),
                    verified_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    provider_id = table.Column<int>(type: "int", nullable: true),
                    last_login_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    created_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_time = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_providers_provider_id",
                        column: x => x.provider_id,
                        principalSchema: "trans",
                        principalTable: "providers",
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

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    application_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    authorization_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    concurrency_token = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    creation_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expiration_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    redemption_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    reference_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_open_iddict_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_open_iddict_tokens_open_iddict_applications_application_id",
                        column: x => x.application_id,
                        principalSchema: "trans",
                        principalTable: "OpenIddictApplications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_open_iddict_tokens_open_iddict_authorizations_authorization_id",
                        column: x => x.authorization_id,
                        principalSchema: "trans",
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "provider_users",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    provider_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    created_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_time = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provider_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_provider_users_providers_provider_id",
                        column: x => x.provider_id,
                        principalSchema: "trans",
                        principalTable: "providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_provider_users_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "trans",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    last_access_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authorization_ids = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    last_authenticated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    created_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_time = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sessions", x => x.id);
                    table.ForeignKey(
                        name: "fk_sessions_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "trans",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_clients",
                schema: "trans",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    app_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    scope = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    created_time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    modified_time = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_clients", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_clients_open_iddict_entity_framework_core_application_string_app_id",
                        column: x => x.app_id,
                        principalSchema: "trans",
                        principalTable: "open_iddict_entity_framework_core_application_string",
                        principalColumn: "client_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_clients_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "trans",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_applications_client_id",
                schema: "trans",
                table: "OpenIddictApplications",
                column: "client_id",
                unique: true,
                filter: "[client_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_authorizations_application_id_status_subject_type",
                schema: "trans",
                table: "OpenIddictAuthorizations",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_scopes_name",
                schema: "trans",
                table: "OpenIddictScopes",
                column: "name",
                unique: true,
                filter: "[name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_application_id_status_subject_type",
                schema: "trans",
                table: "OpenIddictTokens",
                columns: new[] { "application_id", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_authorization_id",
                schema: "trans",
                table: "OpenIddictTokens",
                column: "authorization_id");

            migrationBuilder.CreateIndex(
                name: "ix_open_iddict_tokens_reference_id",
                schema: "trans",
                table: "OpenIddictTokens",
                column: "reference_id",
                unique: true,
                filter: "[reference_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_provider_users_provider_id_subject_id",
                schema: "trans",
                table: "provider_users",
                columns: new[] { "provider_id", "subject_id" },
                unique: true,
                filter: "[subject_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_provider_users_user_id",
                schema: "trans",
                table: "provider_users",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_sessions_user_id",
                schema: "trans",
                table: "sessions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_clients_app_id",
                schema: "trans",
                table: "user_clients",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_clients_user_id",
                schema: "trans",
                table: "user_clients",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "trans",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_provider_id",
                schema: "trans",
                table: "users",
                column: "provider_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "open_iddict_entity_framework_core_token_string",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "OpenIddictScopes",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "OpenIddictTokens",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "provider_users",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "sessions",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "user_clients",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "open_iddict_entity_framework_core_authorization_string",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "users",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "open_iddict_entity_framework_core_application_string",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "OpenIddictApplications",
                schema: "trans");

            migrationBuilder.DropTable(
                name: "providers",
                schema: "trans");
        }
    }
}
