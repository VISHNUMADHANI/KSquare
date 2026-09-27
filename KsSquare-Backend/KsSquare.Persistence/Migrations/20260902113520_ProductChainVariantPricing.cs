using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KsSquare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductChainVariantPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product_chain_variants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chain_size_option_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chain_width_option_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chain_diamond_size_option_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("pk_product_chain_variants", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_chain_variants_product_options_chain_diamond_size_o",
                        column: x => x.chain_diamond_size_option_id,
                        principalTable: "product_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_chain_variants_product_options_chain_size_option_id",
                        column: x => x.chain_size_option_id,
                        principalTable: "product_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_chain_variants_product_options_chain_width_option_id",
                        column: x => x.chain_width_option_id,
                        principalTable: "product_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_chain_variants_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_product_chain_variants_chain_diamond_size_option_id",
                table: "product_chain_variants",
                column: "chain_diamond_size_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_chain_variants_chain_size_option_id",
                table: "product_chain_variants",
                column: "chain_size_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_chain_variants_chain_width_option_id",
                table: "product_chain_variants",
                column: "chain_width_option_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_chain_variants_product_id_chain_size_option_id_chai",
                table: "product_chain_variants",
                columns: new[] { "product_id", "chain_size_option_id", "chain_width_option_id", "chain_diamond_size_option_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_chain_variants");
        }
    }
}
