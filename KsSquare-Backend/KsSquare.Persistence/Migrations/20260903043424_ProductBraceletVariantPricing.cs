using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KsSquare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductBraceletVariantPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_bracelet_variants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bracelet_size_option_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bracelet_stone_size_option_id = table.Column<Guid>(type: "uuid", nullable: false),
                    color_option_id = table.Column<Guid>(type: "uuid", nullable: false),
                    combination_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    original_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_available = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_bracelet_variants", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_bracelet_variants_product_options_bracelet_size_opt",
                        column: x => x.bracelet_size_option_id,
                        principalTable: "product_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_bracelet_variants_product_options_bracelet_stone_si",
                        column: x => x.bracelet_stone_size_option_id,
                        principalTable: "product_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_bracelet_variants_product_options_color_option_id",
                        column: x => x.color_option_id,
                        principalTable: "product_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_bracelet_variants_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_bracelet_variants_bracelet_size_option_id",
                table: "product_bracelet_variants",
                column: "bracelet_size_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_bracelet_variants_bracelet_stone_size_option_id",
                table: "product_bracelet_variants",
                column: "bracelet_stone_size_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_bracelet_variants_color_option_id",
                table: "product_bracelet_variants",
                column: "color_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_bracelet_variants_product_id_combination_key",
                table: "product_bracelet_variants",
                columns: new[] { "product_id", "combination_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_bracelet_variants");
        }
    }
}
