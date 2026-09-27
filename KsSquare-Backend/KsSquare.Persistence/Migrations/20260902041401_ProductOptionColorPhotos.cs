using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KsSquare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductOptionColorPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "color_hex",
                table: "product_options");

            migrationBuilder.AddColumn<long>(
                name: "image_content_length",
                table: "product_options",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_content_type",
                table: "product_options",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "image_object_key",
                table: "product_options",
                type: "character varying(512)",
                maxLength: 512,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "image_content_length",
                table: "product_options");

            migrationBuilder.DropColumn(
                name: "image_content_type",
                table: "product_options");

            migrationBuilder.DropColumn(
                name: "image_object_key",
                table: "product_options");

            migrationBuilder.AddColumn<string>(
                name: "color_hex",
                table: "product_options",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);
        }
    }
}
