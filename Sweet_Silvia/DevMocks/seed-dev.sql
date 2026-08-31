BEGIN;

INSERT INTO "Roles" ("Id", "Name", "IsActive") VALUES
('11111111-1111-1111-1111-111111111111', 'Cliente', TRUE),
('22222222-2222-2222-2222-222222222222', 'Administrador', TRUE),
('33333333-3333-3333-3333-333333333333', 'Asistente', TRUE)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = EXCLUDED."Name",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "Users" ("Id", "RoleId", "Name", "PaternalSurname", "MaternalSurname", "Phone", "Email", "PasswordHash", "Status", "CreatedAt", "UpdatedAt") VALUES
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', '22222222-2222-2222-2222-222222222222', 'Admin', 'Sweet', 'Silvia', '+51911111111', 'admin@sweetsilvia.local', 'PBKDF2-SHA256$210000$AQIDBAUGBwgJCgsMDQ4PEA==$L1qU9qECyJEvZzmOtRqpMOLrUa7x1Kwql3csHWj4Fsk=', 'Active', '2026-08-31T20:00:00Z', NULL),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', '33333333-3333-3333-3333-333333333333', 'Asistente', 'Sweet', 'Silvia', '+51922222222', 'asistente@sweetsilvia.local', 'PBKDF2-SHA256$210000$AQIDBAUGBwgJCgsMDQ4PEA==$5rDG0DXSseGV0azAQva+SjnLvjmGzOm1EkYmKo7aWPk=', 'Active', '2026-08-31T20:00:00Z', NULL),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', '11111111-1111-1111-1111-111111111111', 'Cliente', 'Demo', 'Frontend', '+51933333333', 'cliente@sweetsilvia.local', 'PBKDF2-SHA256$210000$AQIDBAUGBwgJCgsMDQ4PEA==$9pdZfEOg/CCxIsQfvsU8lgd/Wq/MVU4Ww83DbG61yo8=', 'Active', '2026-08-31T20:00:00Z', NULL)
ON CONFLICT ("Id") DO UPDATE SET
"RoleId" = EXCLUDED."RoleId",
"Name" = EXCLUDED."Name",
"PaternalSurname" = EXCLUDED."PaternalSurname",
"MaternalSurname" = EXCLUDED."MaternalSurname",
"Phone" = EXCLUDED."Phone",
"Email" = EXCLUDED."Email",
"PasswordHash" = EXCLUDED."PasswordHash",
"Status" = EXCLUDED."Status";

INSERT INTO "Departments" ("Id", "Name", "DestinationType", "IsActive") VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Lima', 'Lima', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', 'Arequipa', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb003', 'La Libertad', 'Province', TRUE)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = EXCLUDED."Name",
"DestinationType" = EXCLUDED."DestinationType",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "Provinces" ("Id", "DepartmentId", "Name", "IsActive") VALUES
('cccccccc-cccc-cccc-cccc-ccccccccc001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Lima', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc002', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', 'Arequipa', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc003', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb003', 'Trujillo', TRUE)
ON CONFLICT ("Id") DO UPDATE SET
"DepartmentId" = EXCLUDED."DepartmentId",
"Name" = EXCLUDED."Name",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "Districts" ("Id", "ProvinceId", "Name", "Ubigeo", "IsActive") VALUES
('dddddddd-dddd-dddd-dddd-ddddddddd001', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Miraflores', '150122', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd002', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'San Isidro', '150131', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd003', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Santiago de Surco', '150140', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd004', 'cccccccc-cccc-cccc-cccc-ccccccccc002', 'Cercado de Arequipa', '040101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd005', 'cccccccc-cccc-cccc-cccc-ccccccccc003', 'Trujillo', '130101', TRUE)
ON CONFLICT ("Id") DO UPDATE SET
"ProvinceId" = EXCLUDED."ProvinceId",
"Name" = EXCLUDED."Name",
"Ubigeo" = EXCLUDED."Ubigeo",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "Addresses" ("Id", "UserId", "ReceiverName", "ReceiverPhone", "Country", "DepartmentId", "ProvinceId", "DistrictId", "Line", "References", "IsDefault", "IsActive", "CreatedAt", "UpdatedAt") VALUES
('eeeeeeee-eeee-eeee-eeee-eeeeeeeee001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'Cliente Demo', '+51933333333', 'PE', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'dddddddd-dddd-dddd-dddd-ddddddddd001', 'Av. Larco 123', 'Frente al parque Kennedy', TRUE, TRUE, '2026-08-31T20:00:00Z', NULL),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeee002', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'Cliente Demo', '+51933333333', 'PE', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', 'cccccccc-cccc-cccc-cccc-ccccccccc002', 'dddddddd-dddd-dddd-dddd-ddddddddd004', 'Calle Mercaderes 456', 'Segundo piso', FALSE, TRUE, '2026-08-31T20:00:00Z', NULL)
ON CONFLICT ("Id") DO UPDATE SET
"ReceiverName" = EXCLUDED."ReceiverName",
"ReceiverPhone" = EXCLUDED."ReceiverPhone",
"DepartmentId" = EXCLUDED."DepartmentId",
"ProvinceId" = EXCLUDED."ProvinceId",
"DistrictId" = EXCLUDED."DistrictId",
"Line" = EXCLUDED."Line",
"References" = EXCLUDED."References",
"IsDefault" = EXCLUDED."IsDefault",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "Categories" ("Id", "Name", "Description", "IsActive") VALUES
('10000000-0000-0000-0000-000000000001', 'Polos', 'Polos urbanos y basicos.', TRUE),
('10000000-0000-0000-0000-000000000002', 'Jeans', 'Pantalones denim para mujer.', TRUE),
('10000000-0000-0000-0000-000000000003', 'Vestidos', 'Vestidos casuales y de temporada.', TRUE),
('10000000-0000-0000-0000-000000000004', 'Accesorios', 'Complementos para outfits.', TRUE)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = EXCLUDED."Name",
"Description" = EXCLUDED."Description",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "Collections" ("Id", "Name", "Description", "IsActive") VALUES
('20000000-0000-0000-0000-000000000001', 'Verano 2026', 'Prendas frescas de temporada.', TRUE),
('20000000-0000-0000-0000-000000000002', 'Oversize', 'Cortes amplios y comodos.', TRUE),
('20000000-0000-0000-0000-000000000003', 'Basicos', 'Prendas esenciales para todos los dias.', TRUE)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = EXCLUDED."Name",
"Description" = EXCLUDED."Description",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "Products" ("Id", "CategoryId", "CollectionId", "Name", "Description", "BasePrice", "IsActive", "CreatedAt", "UpdatedAt") VALUES
('30000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000002', 'Polo Oversize Blanco', 'Polo oversize de algodon color blanco.', '89.90|PEN', TRUE, '2026-08-31T20:00:00Z', NULL),
('30000000-0000-0000-0000-000000000002', '10000000-0000-0000-0000-000000000002', '20000000-0000-0000-0000-000000000003', 'Jean Wide Leg Azul', 'Jean wide leg azul clasico.', '159.90|PEN', TRUE, '2026-08-31T20:00:00Z', NULL),
('30000000-0000-0000-0000-000000000003', '10000000-0000-0000-0000-000000000003', '20000000-0000-0000-0000-000000000001', 'Vestido Floral Rosa', 'Vestido floral liviano para verano.', '139.90|PEN', TRUE, '2026-08-31T20:00:00Z', NULL),
('30000000-0000-0000-0000-000000000004', '10000000-0000-0000-0000-000000000004', NULL, 'Gorra Sweet Silvia', 'Gorra bordada con logo Sweet Silvia.', '49.90|PEN', TRUE, '2026-08-31T20:00:00Z', NULL)
ON CONFLICT ("Id") DO UPDATE SET
"CategoryId" = EXCLUDED."CategoryId",
"CollectionId" = EXCLUDED."CollectionId",
"Name" = EXCLUDED."Name",
"Description" = EXCLUDED."Description",
"BasePrice" = EXCLUDED."BasePrice",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "ProductVariants" ("Id", "ProductId", "Size", "Color", "Sku", "PhysicalStock", "Price", "IsActive", "CreatedAt", "UpdatedAt") VALUES
('40000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', 'S', 'Blanco', 'POLO-BLA-S', 25, NULL, TRUE, '2026-08-31T20:00:00Z', NULL),
('40000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000001', 'M', 'Blanco', 'POLO-BLA-M', 30, NULL, TRUE, '2026-08-31T20:00:00Z', NULL),
('40000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000001', 'L', 'Negro', 'POLO-NEG-L', 18, '94.90|PEN', TRUE, '2026-08-31T20:00:00Z', NULL),
('40000000-0000-0000-0000-000000000004', '30000000-0000-0000-0000-000000000002', '28', 'Azul', 'JEAN-AZU-28', 14, NULL, TRUE, '2026-08-31T20:00:00Z', NULL),
('40000000-0000-0000-0000-000000000005', '30000000-0000-0000-0000-000000000002', '30', 'Azul', 'JEAN-AZU-30', 12, NULL, TRUE, '2026-08-31T20:00:00Z', NULL),
('40000000-0000-0000-0000-000000000006', '30000000-0000-0000-0000-000000000003', 'M', 'Rosa', 'VEST-ROS-M', 16, NULL, TRUE, '2026-08-31T20:00:00Z', NULL),
('40000000-0000-0000-0000-000000000007', '30000000-0000-0000-0000-000000000004', 'Unica', 'Beige', 'GORR-BEI-U', 40, NULL, TRUE, '2026-08-31T20:00:00Z', NULL)
ON CONFLICT ("Id") DO UPDATE SET
"ProductId" = EXCLUDED."ProductId",
"Size" = EXCLUDED."Size",
"Color" = EXCLUDED."Color",
"Sku" = EXCLUDED."Sku",
"PhysicalStock" = EXCLUDED."PhysicalStock",
"Price" = EXCLUDED."Price",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "ProductImages" ("Id", "ProductId", "ProductVariantId", "Url", "AltText", "Order", "IsMain", "CreatedAt") VALUES
('50000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', NULL, 'https://placehold.co/800x1000?text=Polo+Oversize+Blanco', 'Polo oversize blanco', 1, TRUE, '2026-08-31T20:00:00Z'),
('50000000-0000-0000-0000-000000000002', '30000000-0000-0000-0000-000000000002', NULL, 'https://placehold.co/800x1000?text=Jean+Wide+Leg+Azul', 'Jean wide leg azul', 1, TRUE, '2026-08-31T20:00:00Z'),
('50000000-0000-0000-0000-000000000003', '30000000-0000-0000-0000-000000000003', NULL, 'https://placehold.co/800x1000?text=Vestido+Floral+Rosa', 'Vestido floral rosa', 1, TRUE, '2026-08-31T20:00:00Z'),
('50000000-0000-0000-0000-000000000004', '30000000-0000-0000-0000-000000000004', NULL, 'https://placehold.co/800x1000?text=Gorra+Sweet+Silvia', 'Gorra Sweet Silvia', 1, TRUE, '2026-08-31T20:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET
"Url" = EXCLUDED."Url",
"AltText" = EXCLUDED."AltText",
"Order" = EXCLUDED."Order",
"IsMain" = EXCLUDED."IsMain";

INSERT INTO "PaymentMethods" ("Id", "Name", "Type", "RequiresManualVerification", "RequiresExternalIntegration", "IsActive") VALUES
('60000000-0000-0000-0000-000000000001', 'Yape', 'Manual', TRUE, FALSE, TRUE),
('60000000-0000-0000-0000-000000000002', 'Plin', 'Manual', TRUE, FALSE, TRUE),
('60000000-0000-0000-0000-000000000003', 'Transferencia bancaria', 'Manual', TRUE, FALSE, TRUE),
('60000000-0000-0000-0000-000000000004', 'Izipay', 'Gateway', FALSE, TRUE, TRUE),
('60000000-0000-0000-0000-000000000005', 'Efectivo', 'Cash', FALSE, FALSE, TRUE)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = EXCLUDED."Name",
"Type" = EXCLUDED."Type",
"RequiresManualVerification" = EXCLUDED."RequiresManualVerification",
"RequiresExternalIntegration" = EXCLUDED."RequiresExternalIntegration",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "Couriers" ("Id", "Name", "Type", "RequiresExternalIntegration", "IsActive") VALUES
('70000000-0000-0000-0000-000000000001', 'Delivery Lima', 'OwnDelivery', FALSE, TRUE),
('70000000-0000-0000-0000-000000000002', 'Indrive Lima', 'DeliveryApp', FALSE, TRUE),
('70000000-0000-0000-0000-000000000003', 'Olva Courier', 'Agency', TRUE, TRUE),
('70000000-0000-0000-0000-000000000004', 'Shalom', 'Agency', TRUE, TRUE)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = EXCLUDED."Name",
"Type" = EXCLUDED."Type",
"RequiresExternalIntegration" = EXCLUDED."RequiresExternalIntegration",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "ShippingRates" ("Id", "CourierId", "CourierType", "Country", "DepartmentId", "ProvinceId", "DistrictId", "DestinationType", "ServiceType", "Cost", "FreeShippingMinimumAmount", "EstimatedTime", "IsActive", "CreatedAt", "UpdatedAt") VALUES
('80000000-0000-0000-0000-000000000001', '70000000-0000-0000-0000-000000000001', 'OwnDelivery', 'PE', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'dddddddd-dddd-dddd-dddd-ddddddddd001', 'Lima', 'SameDay', '12.00|PEN', '300.00|PEN', 'Mismo dia', TRUE, '2026-08-31T20:00:00Z', NULL),
('80000000-0000-0000-0000-000000000002', '70000000-0000-0000-0000-000000000002', 'DeliveryApp', 'PE', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'cccccccc-cccc-cccc-cccc-ccccccccc001', NULL, 'Lima', 'Express', '18.00|PEN', '300.00|PEN', '24 horas', TRUE, '2026-08-31T20:00:00Z', NULL),
('80000000-0000-0000-0000-000000000003', '70000000-0000-0000-0000-000000000003', 'Agency', 'PE', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', 'cccccccc-cccc-cccc-cccc-ccccccccc002', NULL, 'Province', 'Agency', '25.00|PEN', '350.00|PEN', '2 a 4 dias', TRUE, '2026-08-31T20:00:00Z', NULL),
('80000000-0000-0000-0000-000000000004', '70000000-0000-0000-0000-000000000004', 'Agency', 'PE', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb003', 'cccccccc-cccc-cccc-cccc-ccccccccc003', NULL, 'Province', 'Agency', '22.00|PEN', '350.00|PEN', '2 a 5 dias', TRUE, '2026-08-31T20:00:00Z', NULL)
ON CONFLICT ("Id") DO UPDATE SET
"CourierId" = EXCLUDED."CourierId",
"CourierType" = EXCLUDED."CourierType",
"DepartmentId" = EXCLUDED."DepartmentId",
"ProvinceId" = EXCLUDED."ProvinceId",
"DistrictId" = EXCLUDED."DistrictId",
"DestinationType" = EXCLUDED."DestinationType",
"ServiceType" = EXCLUDED."ServiceType",
"Cost" = EXCLUDED."Cost",
"FreeShippingMinimumAmount" = EXCLUDED."FreeShippingMinimumAmount",
"EstimatedTime" = EXCLUDED."EstimatedTime",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "Coupons" ("Id", "Code", "Name", "Scope", "DiscountType", "DiscountValue", "MinimumPurchaseAmount", "MinimumItemsQuantity", "MaximumDiscountedItemsQuantity", "ApplicationStrategy", "StartDate", "EndDate", "IsActive", "MaximumTotalUses", "MaximumUsesPerUser") VALUES
('90000000-0000-0000-0000-000000000001', 'POLO20', 'S/ 20 en polos', 'Item', 'Amount', 20.00, NULL, NULL, 1, 'MostExpensive', '2026-08-01T00:00:00Z', '2026-12-31T23:59:59Z', TRUE, NULL, 1),
('90000000-0000-0000-0000-000000000002', 'WELC10', '10% bienvenida', 'Order', 'Percentage', 10.00, 100.00, NULL, NULL, NULL, '2026-08-01T00:00:00Z', '2026-12-31T23:59:59Z', TRUE, 1000, 1),
('90000000-0000-0000-0000-000000000003', 'ENVIO0', 'Envio gratis desde S/ 300', 'Order', 'Amount', 30.00, 300.00, NULL, NULL, NULL, '2026-08-01T00:00:00Z', '2026-12-31T23:59:59Z', TRUE, NULL, NULL)
ON CONFLICT ("Id") DO UPDATE SET
"Code" = EXCLUDED."Code",
"Name" = EXCLUDED."Name",
"Scope" = EXCLUDED."Scope",
"DiscountType" = EXCLUDED."DiscountType",
"DiscountValue" = EXCLUDED."DiscountValue",
"MinimumPurchaseAmount" = EXCLUDED."MinimumPurchaseAmount",
"MinimumItemsQuantity" = EXCLUDED."MinimumItemsQuantity",
"MaximumDiscountedItemsQuantity" = EXCLUDED."MaximumDiscountedItemsQuantity",
"ApplicationStrategy" = EXCLUDED."ApplicationStrategy",
"StartDate" = EXCLUDED."StartDate",
"EndDate" = EXCLUDED."EndDate",
"IsActive" = EXCLUDED."IsActive",
"MaximumTotalUses" = EXCLUDED."MaximumTotalUses",
"MaximumUsesPerUser" = EXCLUDED."MaximumUsesPerUser";

INSERT INTO "CouponRestrictions" ("Id", "CouponId", "CategoryId", "ProductId", "ProductVariantId", "CollectionId") VALUES
('91000000-0000-0000-0000-000000000001', '90000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', NULL, NULL, NULL)
ON CONFLICT ("Id") DO UPDATE SET
"CouponId" = EXCLUDED."CouponId",
"CategoryId" = EXCLUDED."CategoryId",
"ProductId" = EXCLUDED."ProductId",
"ProductVariantId" = EXCLUDED."ProductVariantId",
"CollectionId" = EXCLUDED."CollectionId";

INSERT INTO "Carts" ("Id", "UserId", "CreatedAt", "UpdatedAt", "IsActive") VALUES
('a0000000-0000-0000-0000-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', '2026-08-31T20:00:00Z', '2026-08-31T20:00:00Z', TRUE)
ON CONFLICT ("Id") DO UPDATE SET
"UserId" = EXCLUDED."UserId",
"UpdatedAt" = EXCLUDED."UpdatedAt",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "CartItems" ("Id", "CartId", "ProductVariantId", "Quantity", "AddedAt", "UpdatedAt") VALUES
('a1000000-0000-0000-0000-000000000001', 'a0000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000002', 1, '2026-08-31T20:00:00Z', NULL),
('a1000000-0000-0000-0000-000000000002', 'a0000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000004', 1, '2026-08-31T20:00:00Z', NULL)
ON CONFLICT ("Id") DO UPDATE SET
"CartId" = EXCLUDED."CartId",
"ProductVariantId" = EXCLUDED."ProductVariantId",
"Quantity" = EXCLUDED."Quantity";

INSERT INTO "Orders" ("Id", "UserId", "AddressId", "CouponId", "Status", "CreatedAt", "UpdatedAt", "Subtotal", "DiscountTotal", "ShippingCost", "Total") VALUES
('b0000000-0000-0000-0000-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeee001', NULL, 'PendingReceipt', '2026-08-31T20:00:00Z', NULL, '249.80|PEN', '0.00|PEN', '12.00|PEN', '261.80|PEN')
ON CONFLICT ("Id") DO UPDATE SET
"Status" = EXCLUDED."Status",
"Subtotal" = EXCLUDED."Subtotal",
"DiscountTotal" = EXCLUDED."DiscountTotal",
"ShippingCost" = EXCLUDED."ShippingCost",
"Total" = EXCLUDED."Total";

INSERT INTO "OrderItems" ("Id", "OrderId", "ProductVariantId", "HistoricalProductName", "HistoricalSku", "HistoricalSize", "HistoricalColor", "Quantity", "HistoricalUnitPrice", "HistoricalUnitDiscount", "Subtotal") VALUES
('b1000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000002', 'Polo Oversize Blanco', 'POLO-BLA-M', 'M', 'Blanco', 1, '89.90|PEN', '0.00|PEN', '89.90|PEN'),
('b1000000-0000-0000-0000-000000000002', 'b0000000-0000-0000-0000-000000000001', '40000000-0000-0000-0000-000000000004', 'Jean Wide Leg Azul', 'JEAN-AZU-28', '28', 'Azul', 1, '159.90|PEN', '0.00|PEN', '159.90|PEN')
ON CONFLICT ("Id") DO UPDATE SET
"Quantity" = EXCLUDED."Quantity",
"HistoricalUnitPrice" = EXCLUDED."HistoricalUnitPrice",
"HistoricalUnitDiscount" = EXCLUDED."HistoricalUnitDiscount",
"Subtotal" = EXCLUDED."Subtotal";

INSERT INTO "Payments" ("Id", "OrderId", "PaymentMethodId", "Status", "Amount", "Currency", "TransactionCode", "AuthorizationCode", "CreatedAt", "PaidAt", "ReceiptUploadedAt", "ReviewedAt", "ReviewedByUserId", "Observation") VALUES
('c0000000-0000-0000-0000-000000000001', 'b0000000-0000-0000-0000-000000000001', '60000000-0000-0000-0000-000000000001', 'PendingReceipt', '261.80|PEN', 'PEN', NULL, NULL, '2026-08-31T20:00:00Z', NULL, NULL, NULL, NULL, NULL)
ON CONFLICT ("Id") DO UPDATE SET
"Status" = EXCLUDED."Status",
"Amount" = EXCLUDED."Amount",
"Currency" = EXCLUDED."Currency",
"Observation" = EXCLUDED."Observation";

COMMIT;
