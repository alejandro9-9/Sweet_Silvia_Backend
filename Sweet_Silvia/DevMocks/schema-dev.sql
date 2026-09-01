CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Addresses" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "ReceiverName" character varying(180) NOT NULL,
        "ReceiverPhone" character varying(12) NOT NULL,
        "Country" character varying(2) NOT NULL,
        "DepartmentId" uuid NOT NULL,
        "ProvinceId" uuid NOT NULL,
        "DistrictId" uuid NOT NULL,
        "Line" character varying(250) NOT NULL,
        "References" character varying(500),
        "IsDefault" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Addresses" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "CartAppliedCoupons" (
        "Id" uuid NOT NULL,
        "CartId" uuid NOT NULL,
        "CouponId" uuid NOT NULL,
        "Scope" character varying(40) NOT NULL,
        "CartItemId" uuid,
        "AppliedQuantity" integer,
        "EstimatedDiscountAmount" character varying(40) NOT NULL,
        "AppliedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CartAppliedCoupons" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "CartItems" (
        "Id" uuid NOT NULL,
        "CartId" uuid NOT NULL,
        "ProductVariantId" uuid NOT NULL,
        "Quantity" integer NOT NULL,
        "AddedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_CartItems" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Carts" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Carts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Categories" (
        "Id" uuid NOT NULL,
        "Name" character varying(150) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Collections" (
        "Id" uuid NOT NULL,
        "Name" character varying(150) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Collections" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "CouponRestrictions" (
        "Id" uuid NOT NULL,
        "CouponId" uuid NOT NULL,
        "CategoryId" uuid,
        "ProductId" uuid,
        "ProductVariantId" uuid,
        "CollectionId" uuid,
        CONSTRAINT "PK_CouponRestrictions" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Coupons" (
        "Id" uuid NOT NULL,
        "Code" character varying(30) NOT NULL,
        "Name" character varying(150) NOT NULL,
        "Scope" character varying(40) NOT NULL,
        "DiscountType" character varying(40) NOT NULL,
        "DiscountValue" numeric(12,2) NOT NULL,
        "MinimumPurchaseAmount" numeric(12,2),
        "MinimumItemsQuantity" integer,
        "MaximumDiscountedItemsQuantity" integer,
        "ApplicationStrategy" character varying(40),
        "StartDate" timestamp with time zone NOT NULL,
        "EndDate" timestamp with time zone NOT NULL,
        "IsActive" boolean NOT NULL,
        "MaximumTotalUses" integer,
        "MaximumUsesPerUser" integer,
        CONSTRAINT "PK_Coupons" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "CouponUsages" (
        "Id" uuid NOT NULL,
        "CouponId" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "Scope" character varying(40) NOT NULL,
        "OrderItemId" uuid,
        "AppliedQuantity" integer,
        "DiscountAmount" character varying(40) NOT NULL,
        "UsedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CouponUsages" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Couriers" (
        "Id" uuid NOT NULL,
        "Name" character varying(150) NOT NULL,
        "Type" character varying(40) NOT NULL,
        "RequiresExternalIntegration" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Couriers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260901060716_AddOlvaAgencyCache') THEN
    CREATE TABLE "OlvaAgencies" (
        "Id" uuid NOT NULL,
        "Code" character varying(60) NOT NULL,
        "Name" character varying(180) NOT NULL,
        "Type" character varying(80),
        "Department" character varying(120),
        "Province" character varying(120),
        "District" character varying(120),
        "Address" character varying(300),
        "Phone" character varying(80),
        "ScheduleJson" jsonb,
        "Latitude" double precision,
        "Longitude" double precision,
        "IsActive" boolean NOT NULL,
        "LastSyncedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_OlvaAgencies" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Departments" (
        "Id" uuid NOT NULL,
        "Name" character varying(120) NOT NULL,
        "DestinationType" character varying(40) NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Departments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Districts" (
        "Id" uuid NOT NULL,
        "ProvinceId" uuid NOT NULL,
        "Name" character varying(120) NOT NULL,
        "Ubigeo" character varying(10),
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Districts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "OrderItems" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "ProductVariantId" uuid NOT NULL,
        "HistoricalProductName" character varying(180) NOT NULL,
        "HistoricalSku" character varying(50) NOT NULL,
        "HistoricalSize" character varying(40) NOT NULL,
        "HistoricalColor" character varying(80) NOT NULL,
        "Quantity" integer NOT NULL,
        "HistoricalUnitPrice" character varying(40) NOT NULL,
        "HistoricalUnitDiscount" character varying(40) NOT NULL,
        "Subtotal" character varying(40) NOT NULL,
        CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Orders" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "AddressId" uuid NOT NULL,
        "CouponId" uuid,
        "Status" character varying(40) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        "Subtotal" character varying(40) NOT NULL,
        "DiscountTotal" character varying(40) NOT NULL,
        "ShippingCost" character varying(40) NOT NULL,
        "Total" character varying(40) NOT NULL,
        CONSTRAINT "PK_Orders" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "OrderStatusHistories" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "PreviousStatus" character varying(40),
        "NewStatus" character varying(40) NOT NULL,
        "ChangedByUserId" uuid,
        "ChangedAt" timestamp with time zone NOT NULL,
        "Observation" character varying(500),
        CONSTRAINT "PK_OrderStatusHistories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "PaymentMethods" (
        "Id" uuid NOT NULL,
        "Name" character varying(120) NOT NULL,
        "Type" character varying(40) NOT NULL,
        "RequiresManualVerification" boolean NOT NULL,
        "RequiresExternalIntegration" boolean NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_PaymentMethods" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "PaymentReceipts" (
        "Id" uuid NOT NULL,
        "PaymentId" uuid NOT NULL,
        "FileUrl" character varying(1000) NOT NULL,
        "OperationCode" character varying(120),
        "DeclaredAmount" character varying(40),
        "UploadedAt" timestamp with time zone NOT NULL,
        "Status" character varying(40) NOT NULL,
        "Observation" character varying(500),
        CONSTRAINT "PK_PaymentReceipts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "PaymentReviews" (
        "Id" uuid NOT NULL,
        "PaymentId" uuid NOT NULL,
        "ReviewerUserId" uuid NOT NULL,
        "Result" character varying(40) NOT NULL,
        "ReviewedAt" timestamp with time zone NOT NULL,
        "Observation" character varying(500),
        CONSTRAINT "PK_PaymentReviews" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Payments" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "PaymentMethodId" uuid NOT NULL,
        "Status" character varying(40) NOT NULL,
        "Amount" character varying(40) NOT NULL,
        "Currency" character varying(3) NOT NULL,
        "TransactionCode" character varying(120),
        "AuthorizationCode" character varying(120),
        "CreatedAt" timestamp with time zone NOT NULL,
        "PaidAt" timestamp with time zone,
        "ReceiptUploadedAt" timestamp with time zone,
        "ReviewedAt" timestamp with time zone,
        "ReviewedByUserId" uuid,
        "Observation" character varying(500),
        CONSTRAINT "PK_Payments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "ProductImages" (
        "Id" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "ProductVariantId" uuid,
        "Url" character varying(1000) NOT NULL,
        "AltText" character varying(250),
        "Order" integer NOT NULL,
        "IsMain" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ProductImages" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Products" (
        "Id" uuid NOT NULL,
        "CategoryId" uuid NOT NULL,
        "CollectionId" uuid,
        "Name" character varying(180) NOT NULL,
        "Description" character varying(1000),
        "BasePrice" character varying(40) NOT NULL,
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Products" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "ProductVariants" (
        "Id" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "Size" character varying(40) NOT NULL,
        "Color" character varying(80) NOT NULL,
        "Sku" character varying(50) NOT NULL,
        "PhysicalStock" integer NOT NULL,
        "Price" character varying(40),
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ProductVariants" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Provinces" (
        "Id" uuid NOT NULL,
        "DepartmentId" uuid NOT NULL,
        "Name" character varying(120) NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Provinces" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "RefreshTokens" (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "TokenHash" character varying(512) NOT NULL,
        "ExpiresAt" timestamp with time zone NOT NULL,
        "RevokedAt" timestamp with time zone,
        "DeviceInfo" character varying(500),
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Roles" (
        "Id" uuid NOT NULL,
        "Name" character varying(80) NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "ShipmentEvents" (
        "Id" uuid NOT NULL,
        "ShipmentId" uuid NOT NULL,
        "Status" character varying(40) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "Location" character varying(180),
        "EventDate" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ShipmentEvents" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Shipments" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "CourierId" uuid NOT NULL,
        "Status" character varying(40) NOT NULL,
        "TrackingCode" character varying(120),
        "ExternalShipmentCode" character varying(120),
        "ShippingCost" character varying(40) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "RegisteredExternallyAt" timestamp with time zone,
        "EstimatedDeliveryAt" timestamp with time zone,
        "DeliveredAt" timestamp with time zone,
        "Observation" character varying(500),
        CONSTRAINT "PK_Shipments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "ShippingRates" (
        "Id" uuid NOT NULL,
        "CourierId" uuid NOT NULL,
        "CourierType" character varying(40) NOT NULL,
        "Country" character varying(2) NOT NULL,
        "DepartmentId" uuid NOT NULL,
        "ProvinceId" uuid,
        "DistrictId" uuid,
        "DestinationType" character varying(40) NOT NULL,
        "ServiceType" character varying(40) NOT NULL,
        "Cost" character varying(40) NOT NULL,
        "FreeShippingMinimumAmount" character varying(40),
        "EstimatedTime" character varying(80),
        "IsActive" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_ShippingRates" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "StockMovements" (
        "Id" uuid NOT NULL,
        "ProductVariantId" uuid NOT NULL,
        "OrderId" uuid,
        "Type" character varying(40) NOT NULL,
        "Quantity" integer NOT NULL,
        "PreviousStock" integer NOT NULL,
        "NewStock" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "Observation" character varying(500),
        CONSTRAINT "PK_StockMovements" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "RoleId" uuid NOT NULL,
        "Name" character varying(120) NOT NULL,
        "PaternalSurname" character varying(120) NOT NULL,
        "MaternalSurname" character varying(120),
        "Phone" character varying(12) NOT NULL,
        "Email" character varying(255) NOT NULL,
        "PasswordHash" character varying(512) NOT NULL,
        "Status" character varying(40) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    INSERT INTO "Roles" ("Id", "IsActive", "Name")
    VALUES ('11111111-1111-1111-1111-111111111111', TRUE, 'Cliente');
    INSERT INTO "Roles" ("Id", "IsActive", "Name")
    VALUES ('22222222-2222-2222-2222-222222222222', TRUE, 'Administrador');
    INSERT INTO "Roles" ("Id", "IsActive", "Name")
    VALUES ('33333333-3333-3333-3333-333333333333', TRUE, 'Asistente');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Addresses_DepartmentId_ProvinceId_DistrictId" ON "Addresses" ("DepartmentId", "ProvinceId", "DistrictId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Addresses_UserId" ON "Addresses" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_CartAppliedCoupons_CartId" ON "CartAppliedCoupons" ("CartId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_CartAppliedCoupons_CouponId" ON "CartAppliedCoupons" ("CouponId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_CartItems_CartId_ProductVariantId" ON "CartItems" ("CartId", "ProductVariantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Carts_UserId_IsActive" ON "Carts" ("UserId", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_CouponRestrictions_CategoryId" ON "CouponRestrictions" ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_CouponRestrictions_CollectionId" ON "CouponRestrictions" ("CollectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_CouponRestrictions_CouponId" ON "CouponRestrictions" ("CouponId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_CouponRestrictions_ProductId" ON "CouponRestrictions" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_CouponRestrictions_ProductVariantId" ON "CouponRestrictions" ("ProductVariantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Coupons_Code" ON "Coupons" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Coupons_IsActive_StartDate_EndDate" ON "Coupons" ("IsActive", "StartDate", "EndDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_CouponUsages_CouponId_UserId" ON "CouponUsages" ("CouponId", "UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_CouponUsages_OrderId" ON "CouponUsages" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Districts_ProvinceId_Name" ON "Districts" ("ProvinceId", "Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_OrderItems_OrderId" ON "OrderItems" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_OrderItems_ProductVariantId" ON "OrderItems" ("ProductVariantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260901060716_AddOlvaAgencyCache') THEN
    CREATE UNIQUE INDEX "IX_OlvaAgencies_Code" ON "OlvaAgencies" ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260901060716_AddOlvaAgencyCache') THEN
    CREATE INDEX "IX_OlvaAgencies_Department_Province_District_IsActive" ON "OlvaAgencies" ("Department", "Province", "District", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Orders_AddressId" ON "Orders" ("AddressId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Orders_CouponId" ON "Orders" ("CouponId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Orders_Status" ON "Orders" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Orders_UserId" ON "Orders" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_OrderStatusHistories_OrderId" ON "OrderStatusHistories" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_PaymentReceipts_PaymentId" ON "PaymentReceipts" ("PaymentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_PaymentReviews_PaymentId" ON "PaymentReviews" ("PaymentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_PaymentReviews_ReviewerUserId" ON "PaymentReviews" ("ReviewerUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Payments_OrderId" ON "Payments" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Payments_PaymentMethodId" ON "Payments" ("PaymentMethodId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Payments_Status" ON "Payments" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_ProductImages_ProductId" ON "ProductImages" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_ProductImages_ProductVariantId" ON "ProductImages" ("ProductVariantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Products_CollectionId" ON "Products" ("CollectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_ProductVariants_ProductId" ON "ProductVariants" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_ProductVariants_Sku" ON "ProductVariants" ("Sku");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Provinces_DepartmentId_Name" ON "Provinces" ("DepartmentId", "Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_RefreshTokens_TokenHash" ON "RefreshTokens" ("TokenHash");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Roles_Name" ON "Roles" ("Name");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_ShipmentEvents_ShipmentId" ON "ShipmentEvents" ("ShipmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Shipments_CourierId" ON "Shipments" ("CourierId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Shipments_OrderId" ON "Shipments" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Shipments_Status" ON "Shipments" ("Status");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_Shipments_TrackingCode" ON "Shipments" ("TrackingCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_ShippingRates_DepartmentId_ProvinceId_DistrictId" ON "ShippingRates" ("DepartmentId", "ProvinceId", "DistrictId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_ShippingRates_DestinationType_ServiceType_IsActive" ON "ShippingRates" ("DestinationType", "ServiceType", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_StockMovements_OrderId" ON "StockMovements" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_StockMovements_ProductVariantId" ON "StockMovements" ("ProductVariantId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE INDEX "IX_StockMovements_Type" ON "StockMovements" ("Type");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831200509_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260831200509_InitialCreate', '10.0.11');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831210627_AddAuditLogsAndSessionMetadata') THEN
    ALTER TABLE "RefreshTokens" ADD "IpAddress" character varying(64);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831210627_AddAuditLogsAndSessionMetadata') THEN
    ALTER TABLE "RefreshTokens" ADD "LastUsedAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831210627_AddAuditLogsAndSessionMetadata') THEN
    ALTER TABLE "RefreshTokens" ADD "UserAgent" character varying(500);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831210627_AddAuditLogsAndSessionMetadata') THEN
    CREATE TABLE "AuditLogs" (
        "Id" uuid NOT NULL,
        "ActorUserId" uuid,
        "ActorRole" character varying(80),
        "Action" character varying(120) NOT NULL,
        "EntityName" character varying(120) NOT NULL,
        "EntityId" uuid,
        "OccurredAt" timestamp with time zone NOT NULL,
        "IpAddress" character varying(64),
        "UserAgent" character varying(500),
        "OldValuesJson" jsonb,
        "NewValuesJson" jsonb,
        "Observation" character varying(500),
        CONSTRAINT "PK_AuditLogs" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831210627_AddAuditLogsAndSessionMetadata') THEN
    CREATE INDEX "IX_AuditLogs_ActorUserId" ON "AuditLogs" ("ActorUserId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831210627_AddAuditLogsAndSessionMetadata') THEN
    CREATE INDEX "IX_AuditLogs_EntityName_EntityId" ON "AuditLogs" ("EntityName", "EntityId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831210627_AddAuditLogsAndSessionMetadata') THEN
    CREATE INDEX "IX_AuditLogs_OccurredAt" ON "AuditLogs" ("OccurredAt");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260831210627_AddAuditLogsAndSessionMetadata') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260831210627_AddAuditLogsAndSessionMetadata', '10.0.11');
    END IF;
END $EF$;
COMMIT;

