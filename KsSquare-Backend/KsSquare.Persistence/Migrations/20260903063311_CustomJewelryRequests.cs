using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KsSquare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CustomJewelryRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "custom_jewelry_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    request_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    access_token_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    fixed_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_jewelry_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_custom_jewelry_requests_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "custom_jewelry_request_media",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    custom_jewelry_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    object_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    content_length = table.Column<long>(type: "bigint", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_jewelry_request_media", x => x.id);
                    table.ForeignKey(
                        name: "fk_custom_jewelry_request_media_custom_jewelry_requests_custom",
                        column: x => x.custom_jewelry_request_id,
                        principalTable: "custom_jewelry_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "custom_jewelry_request_options",
                columns: table => new
                {
                    custom_jewelry_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    options_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_jewelry_request_options", x => new { x.custom_jewelry_request_id, x.options_id });
                    table.ForeignKey(
                        name: "fk_custom_jewelry_request_options_custom_jewelry_requests_cust",
                        column: x => x.custom_jewelry_request_id,
                        principalTable: "custom_jewelry_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_custom_jewelry_request_options_product_options_options_id",
                        column: x => x.options_id,
                        principalTable: "product_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_custom_jewelry_request_media_custom_jewelry_request_id",
                table: "custom_jewelry_request_media",
                column: "custom_jewelry_request_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_jewelry_request_media_object_key",
                table: "custom_jewelry_request_media",
                column: "object_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_custom_jewelry_request_options_options_id",
                table: "custom_jewelry_request_options",
                column: "options_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_jewelry_requests_category_id",
                table: "custom_jewelry_requests",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_jewelry_requests_request_number",
                table: "custom_jewelry_requests",
                column: "request_number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "custom_jewelry_request_media");

            migrationBuilder.DropTable(
                name: "custom_jewelry_request_options");

            migrationBuilder.DropTable(
                name: "custom_jewelry_requests");
        }
    }
}
