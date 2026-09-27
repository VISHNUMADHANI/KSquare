using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KsSquare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ImportedCategoryReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "product_id",
                table: "product_reviews",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "order_id",
                table: "product_reviews",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "customer_id",
                table: "product_reviews",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "category_id",
                table: "product_reviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "imported_by",
                table: "product_reviews",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_reviews_category_id_status",
                table: "product_reviews",
                columns: new[] { "category_id", "status" });

            migrationBuilder.AddForeignKey(
                name: "fk_product_reviews_categories_category_id",
                table: "product_reviews",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_product_reviews_categories_category_id",
                table: "product_reviews");

            migrationBuilder.DropIndex(
                name: "ix_product_reviews_category_id_status",
                table: "product_reviews");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "product_reviews");

            migrationBuilder.DropColumn(
                name: "imported_by",
                table: "product_reviews");

            migrationBuilder.AlterColumn<Guid>(
                name: "product_id",
                table: "product_reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "order_id",
                table: "product_reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "customer_id",
                table: "product_reviews",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
