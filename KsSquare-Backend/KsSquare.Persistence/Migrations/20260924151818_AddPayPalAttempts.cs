using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KsSquare.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPayPalAttempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "paypal_attempts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checkout_key = table.Column<Guid>(type: "uuid", nullable: false),
                    request_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    checkout_json = table.Column<string>(type: "jsonb", nullable: false),
                    items_json = table.Column<string>(type: "jsonb", nullable: false),
                    shipping_json = table.Column<string>(type: "jsonb", nullable: false),
                    customer_name = table.Column<string>(type: "text", nullable: false),
                    customer_email = table.Column<string>(type: "text", nullable: false),
                    customer_phone = table.Column<string>(type: "text", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    environment = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    custom_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pay_pal_order_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    approval_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    capture_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_paypal_attempts", x => x.id);
                    table.ForeignKey(
                        name: "fk_paypal_attempts_customer_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "customer_orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_paypal_attempts_users_customer_id",
                        column: x => x.customer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_paypal_attempts_capture_id",
                table: "paypal_attempts",
                column: "capture_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_paypal_attempts_custom_request_id",
                table: "paypal_attempts",
                column: "custom_request_id",
                unique: true,
                filter: "state <> 'Abandoned'");

            migrationBuilder.CreateIndex(
                name: "ix_paypal_attempts_customer_id",
                table: "paypal_attempts",
                column: "customer_id",
                unique: true,
                filter: "state NOT IN ('Completed', 'Abandoned')");

            migrationBuilder.CreateIndex(
                name: "ix_paypal_attempts_customer_id_checkout_key",
                table: "paypal_attempts",
                columns: new[] { "customer_id", "checkout_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_paypal_attempts_order_id",
                table: "paypal_attempts",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_paypal_attempts_pay_pal_order_id",
                table: "paypal_attempts",
                column: "pay_pal_order_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "paypal_attempts");
        }
    }
}
