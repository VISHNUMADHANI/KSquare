using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KsSquare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class OptionalChainPricingDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_product_chain_variants_product_id_chain_size_option_id_chai",
                table: "product_chain_variants");

            migrationBuilder.AlterColumn<Guid>(
                name: "chain_width_option_id",
                table: "product_chain_variants",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "chain_diamond_size_option_id",
                table: "product_chain_variants",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "combination_key",
                table: "product_chain_variants",
                type: "character varying(110)",
                maxLength: 110,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE product_chain_variants
                SET combination_key = replace(chain_size_option_id::text, '-', '') || ':' ||
                    COALESCE(replace(chain_width_option_id::text, '-', ''), 'none') || ':' ||
                    COALESCE(replace(chain_diamond_size_option_id::text, '-', ''), 'none');
                """);

            migrationBuilder.CreateIndex(
                name: "ix_product_chain_variants_product_id_combination_key",
                table: "product_chain_variants",
                columns: new[] { "product_id", "combination_key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_product_chain_variants_product_id_combination_key",
                table: "product_chain_variants");

            migrationBuilder.DropColumn(
                name: "combination_key",
                table: "product_chain_variants");

            migrationBuilder.Sql("DELETE FROM product_chain_variants WHERE chain_width_option_id IS NULL OR chain_diamond_size_option_id IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "chain_width_option_id",
                table: "product_chain_variants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "chain_diamond_size_option_id",
                table: "product_chain_variants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_chain_variants_product_id_chain_size_option_id_chai",
                table: "product_chain_variants",
                columns: new[] { "product_id", "chain_size_option_id", "chain_width_option_id", "chain_diamond_size_option_id" },
                unique: true);
        }
    }
}
