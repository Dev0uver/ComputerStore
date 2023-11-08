using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ComputerStore.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("category_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", maxLength: 25, nullable: false),
                    subcategory = table.Column<int>(type: "integer", maxLength: 25, nullable: false),
                    availability = table.Column<bool>(type: "boolean", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("product_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subcategory",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("subcategory_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    userName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cart",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    productId = table.Column<int>(type: "integer", nullable: false),
                    productName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    cost = table.Column<int>(type: "integer", nullable: false),
                    userId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("cart_pkey", x => x.id);
                    table.ForeignKey(
                        name: "productIdKey",
                        column: x => x.productId,
                        principalTable: "product",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "userIdKey",
                        column: x => x.userId,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    deliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    orderStatus = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    paymentStatus = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    paymentMethod = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    total = table.Column<int>(type: "integer", nullable: false),
                    userId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("order_pkey", x => x.id);
                    table.ForeignKey(
                        name: "userIdKey",
                        column: x => x.userId,
                        principalTable: "user",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "orderItems",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "nextval('items_id_seq'::regclass)"),
                    orderNumber = table.Column<int>(type: "integer", nullable: false),
                    productId = table.Column<int>(type: "integer", nullable: false),
                    productName = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    cost = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("orderItems_pkey", x => x.id);
                    table.ForeignKey(
                        name: "orderIdKey",
                        column: x => x.orderNumber,
                        principalTable: "order",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "productIdKey",
                        column: x => x.productId,
                        principalTable: "product",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_cart_productId",
                table: "cart",
                column: "productId");

            migrationBuilder.CreateIndex(
                name: "IX_cart_userId",
                table: "cart",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_order_userId",
                table: "order",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_orderItems_orderNumber",
                table: "orderItems",
                column: "orderNumber");

            migrationBuilder.CreateIndex(
                name: "IX_orderItems_productId",
                table: "orderItems",
                column: "productId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cart");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "orderItems");

            migrationBuilder.DropTable(
                name: "subcategory");

            migrationBuilder.DropTable(
                name: "order");

            migrationBuilder.DropTable(
                name: "product");

            migrationBuilder.DropTable(
                name: "user");

            migrationBuilder.DropSequence(
                name: "cart_id_seq");

            migrationBuilder.DropSequence(
                name: "category_id_seq");

            migrationBuilder.DropSequence(
                name: "items_id_seq");

            migrationBuilder.DropSequence(
                name: "order_id_seq");

            migrationBuilder.DropSequence(
                name: "subcategory_id_seq");
        }
    }
}
