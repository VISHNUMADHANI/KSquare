using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KsSquare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogCustomizationOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "image_content_length",
                table: "categories",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_content_type",
                table: "categories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_object_key",
                table: "categories",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "show_in_custom",
                table: "categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "product_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    color_hex = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_options", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_option_assignments",
                columns: table => new
                {
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_option_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_option_assignments", x => new { x.product_id, x.product_option_id });
                    table.ForeignKey(
                        name: "fk_product_option_assignments_product_options_product_option_id",
                        column: x => x.product_option_id,
                        principalTable: "product_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_option_assignments_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_option_assignments_product_option_id",
                table: "product_option_assignments",
                column: "product_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_options_type_is_active_display_order",
                table: "product_options",
                columns: new[] { "type", "is_active", "display_order" });

            migrationBuilder.CreateIndex(
                name: "ix_product_options_type_name",
                table: "product_options",
                columns: new[] { "type", "name" },
                unique: true,
                filter: "deleted_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_option_assignments");

            migrationBuilder.DropTable(
                name: "product_options");

            migrationBuilder.DropColumn(
                name: "image_content_length",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "image_content_type",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "image_object_key",
                table: "categories");

            migrationBuilder.DropColumn(
                name: "show_in_custom",
                table: "categories");
        }
    }
}
