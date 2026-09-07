using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sweet_Silvia_Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenDataIntegrityAndConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "GatewayExpiresAt",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingRates_CourierId",
                table: "ShippingRates",
                column: "CourierId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingRates_DistrictId",
                table: "ShippingRates",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingRates_ProvinceId",
                table: "ShippingRates",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_ChangedByUserId",
                table: "OrderStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_OrderItemId",
                table: "CouponUsages",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponUsages_UserId",
                table: "CouponUsages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ProductVariantId",
                table: "CartItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CartAppliedCoupons_CartItemId",
                table: "CartAppliedCoupons",
                column: "CartItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_DistrictId",
                table: "Addresses",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_ProvinceId",
                table: "Addresses",
                column: "ProvinceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Departments_DepartmentId",
                table: "Addresses",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Districts_DistrictId",
                table: "Addresses",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Provinces_ProvinceId",
                table: "Addresses",
                column: "ProvinceId",
                principalTable: "Provinces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Users_UserId",
                table: "Addresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CartAppliedCoupons_CartItems_CartItemId",
                table: "CartAppliedCoupons",
                column: "CartItemId",
                principalTable: "CartItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CartAppliedCoupons_Carts_CartId",
                table: "CartAppliedCoupons",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartAppliedCoupons_Coupons_CouponId",
                table: "CartAppliedCoupons",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductVariants_ProductVariantId",
                table: "CartItems",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Users_UserId",
                table: "Carts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponRestrictions_Categories_CategoryId",
                table: "CouponRestrictions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponRestrictions_Collections_CollectionId",
                table: "CouponRestrictions",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponRestrictions_Coupons_CouponId",
                table: "CouponRestrictions",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponRestrictions_ProductVariants_ProductVariantId",
                table: "CouponRestrictions",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponRestrictions_Products_ProductId",
                table: "CouponRestrictions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponUsages_Coupons_CouponId",
                table: "CouponUsages",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponUsages_OrderItems_OrderItemId",
                table: "CouponUsages",
                column: "OrderItemId",
                principalTable: "OrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponUsages_Orders_OrderId",
                table: "CouponUsages",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CouponUsages_Users_UserId",
                table: "CouponUsages",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_ProductVariants_ProductVariantId",
                table: "OrderItems",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Addresses_AddressId",
                table: "Orders",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders",
                column: "CouponId",
                principalTable: "Coupons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistories_Orders_OrderId",
                table: "OrderStatusHistories",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderStatusHistories_Users_ChangedByUserId",
                table: "OrderStatusHistories",
                column: "ChangedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentReceipts_Payments_PaymentId",
                table: "PaymentReceipts",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentReviews_Payments_PaymentId",
                table: "PaymentReviews",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentReviews_Users_ReviewerUserId",
                table: "PaymentReviews",
                column: "ReviewerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentMethods_PaymentMethodId",
                table: "Payments",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductVariants_ProductVariantId",
                table: "ProductImages",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Collections_CollectionId",
                table: "Products",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariants_Products_ProductId",
                table: "ProductVariants",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentEvents_Shipments_ShipmentId",
                table: "ShipmentEvents",
                column: "ShipmentId",
                principalTable: "Shipments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Couriers_CourierId",
                table: "Shipments",
                column: "CourierId",
                principalTable: "Couriers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Orders_OrderId",
                table: "Shipments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRates_Couriers_CourierId",
                table: "ShippingRates",
                column: "CourierId",
                principalTable: "Couriers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRates_Departments_DepartmentId",
                table: "ShippingRates",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRates_Districts_DistrictId",
                table: "ShippingRates",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShippingRates_Provinces_ProvinceId",
                table: "ShippingRates",
                column: "ProvinceId",
                principalTable: "Provinces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Orders_OrderId",
                table: "StockMovements",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_ProductVariants_ProductVariantId",
                table: "StockMovements",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Departments_DepartmentId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Districts_DistrictId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Provinces_ProvinceId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Users_UserId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_CartAppliedCoupons_CartItems_CartItemId",
                table: "CartAppliedCoupons");

            migrationBuilder.DropForeignKey(
                name: "FK_CartAppliedCoupons_Carts_CartId",
                table: "CartAppliedCoupons");

            migrationBuilder.DropForeignKey(
                name: "FK_CartAppliedCoupons_Coupons_CouponId",
                table: "CartAppliedCoupons");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Carts_CartId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductVariants_ProductVariantId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Users_UserId",
                table: "Carts");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponRestrictions_Categories_CategoryId",
                table: "CouponRestrictions");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponRestrictions_Collections_CollectionId",
                table: "CouponRestrictions");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponRestrictions_Coupons_CouponId",
                table: "CouponRestrictions");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponRestrictions_ProductVariants_ProductVariantId",
                table: "CouponRestrictions");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponRestrictions_Products_ProductId",
                table: "CouponRestrictions");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponUsages_Coupons_CouponId",
                table: "CouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponUsages_OrderItems_OrderItemId",
                table: "CouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponUsages_Orders_OrderId",
                table: "CouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_CouponUsages_Users_UserId",
                table: "CouponUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Orders_OrderId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_ProductVariants_ProductVariantId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Addresses_AddressId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Coupons_CouponId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Users_UserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistories_Orders_OrderId",
                table: "OrderStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderStatusHistories_Users_ChangedByUserId",
                table: "OrderStatusHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReceipts_Payments_PaymentId",
                table: "PaymentReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReviews_Payments_PaymentId",
                table: "PaymentReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReviews_Users_ReviewerUserId",
                table: "PaymentReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Orders_OrderId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentMethods_PaymentMethodId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductVariants_ProductVariantId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Collections_CollectionId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariants_Products_ProductId",
                table: "ProductVariants");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                table: "RefreshTokens");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentEvents_Shipments_ShipmentId",
                table: "ShipmentEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Couriers_CourierId",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Orders_OrderId",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRates_Couriers_CourierId",
                table: "ShippingRates");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRates_Departments_DepartmentId",
                table: "ShippingRates");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRates_Districts_DistrictId",
                table: "ShippingRates");

            migrationBuilder.DropForeignKey(
                name: "FK_ShippingRates_Provinces_ProvinceId",
                table: "ShippingRates");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Orders_OrderId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_ProductVariants_ProductVariantId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_ShippingRates_CourierId",
                table: "ShippingRates");

            migrationBuilder.DropIndex(
                name: "IX_ShippingRates_DistrictId",
                table: "ShippingRates");

            migrationBuilder.DropIndex(
                name: "IX_ShippingRates_ProvinceId",
                table: "ShippingRates");

            migrationBuilder.DropIndex(
                name: "IX_OrderStatusHistories_ChangedByUserId",
                table: "OrderStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_CouponUsages_OrderItemId",
                table: "CouponUsages");

            migrationBuilder.DropIndex(
                name: "IX_CouponUsages_UserId",
                table: "CouponUsages");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ProductVariantId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartAppliedCoupons_CartItemId",
                table: "CartAppliedCoupons");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_DistrictId",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_ProvinceId",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "GatewayExpiresAt",
                table: "Payments");
        }
    }
}
