using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KsSquare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FlexibleBraceletPricingDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_bracelet_variants_product_options_color_option_id",
                table: "product_bracelet_variants");

            migrationBuilder.DropIndex(
                name: "ix_product_bracelet_variants_color_option_id",
                table: "product_bracelet_variants");

            migrationBuilder.DropColumn(
                name: "color_option_id",
                table: "product_bracelet_variants");

            migrationBuilder.AlterColumn<Guid>(
                name: "bracelet_stone_size_option_id",
                table: "product_bracelet_variants",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "bracelet_size_option_id",
                table: "product_bracelet_variants",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "bracelet_stone_size_option_id",
                table: "product_bracelet_variants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "bracelet_size_option_id",
                table: "product_bracelet_variants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "color_option_id",
                table: "product_bracelet_variants",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_product_bracelet_variants_color_option_id",
                table: "product_bracelet_variants",
                column: "color_option_id");

            migrationBuilder.AddForeignKey(
                name: "fk_product_bracelet_variants_product_options_color_option_id",
                table: "product_bracelet_variants",
                column: "color_option_id",
                principalTable: "product_options",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
