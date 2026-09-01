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

INSERT INTO "RefreshTokens" ("Id", "UserId", "TokenHash", "ExpiresAt", "RevokedAt", "DeviceInfo", "IpAddress", "UserAgent", "LastUsedAt", "CreatedAt") VALUES
('ab000000-0000-0000-0000-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'mock-admin-token-hash-001', '2026-09-30T20:00:00Z', NULL, 'Chrome Windows', '192.168.1.10', 'Mozilla/5.0 Mock Admin Browser', '2026-08-31T20:20:00Z', '2026-08-31T20:00:00Z'),
('ab000000-0000-0000-0000-000000000002', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'mock-assistant-token-hash-001', '2026-09-30T20:00:00Z', NULL, 'Chrome Windows', '192.168.1.11', 'Mozilla/5.0 Mock Assistant Browser', '2026-08-31T20:15:00Z', '2026-08-31T20:00:00Z'),
('ab000000-0000-0000-0000-000000000003', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'mock-customer-token-hash-001', '2026-09-30T20:00:00Z', NULL, 'Mobile Android', '192.168.1.20', 'Mozilla/5.0 Mock Customer Mobile', '2026-08-31T20:10:00Z', '2026-08-31T20:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET
"UserId" = EXCLUDED."UserId",
"TokenHash" = EXCLUDED."TokenHash",
"ExpiresAt" = EXCLUDED."ExpiresAt",
"RevokedAt" = EXCLUDED."RevokedAt",
"DeviceInfo" = EXCLUDED."DeviceInfo",
"IpAddress" = EXCLUDED."IpAddress",
"UserAgent" = EXCLUDED."UserAgent",
"LastUsedAt" = EXCLUDED."LastUsedAt",
"CreatedAt" = EXCLUDED."CreatedAt";

INSERT INTO "Departments" ("Id", "Name", "DestinationType", "IsActive") VALUES
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Lima', 'Lima', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', 'Arequipa', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb003', 'La Libertad', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb004', 'Callao', 'Lima', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb005', 'Amazonas', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb006', 'Ancash', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb007', 'Apurimac', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb008', 'Ayacucho', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb009', 'Cajamarca', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb010', 'Cusco', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb011', 'Huancavelica', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb012', 'Huanuco', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb013', 'Ica', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb014', 'Junin', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb015', 'Lambayeque', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb016', 'Loreto', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb017', 'Madre de Dios', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb018', 'Moquegua', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb019', 'Pasco', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb020', 'Piura', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb021', 'Puno', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb022', 'San Martin', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb023', 'Tacna', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb024', 'Tumbes', 'Province', TRUE),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb025', 'Ucayali', 'Province', TRUE)
ON CONFLICT ("Id") DO UPDATE SET
"Name" = EXCLUDED."Name",
"DestinationType" = EXCLUDED."DestinationType",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "Provinces" ("Id", "DepartmentId", "Name", "IsActive") VALUES
('cccccccc-cccc-cccc-cccc-ccccccccc001', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Lima', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc002', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', 'Arequipa', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc003', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb003', 'Trujillo', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc004', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb004', 'Callao', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc005', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Barranca', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc006', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Cajatambo', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc007', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Canta', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc008', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Canete', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc009', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Huaral', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc010', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Huarochiri', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc011', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Huaura', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc012', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Oyon', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc013', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb001', 'Yauyos', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc014', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb005', 'Chachapoyas', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc015', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb006', 'Huaraz', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc016', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb007', 'Abancay', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc017', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb008', 'Huamanga', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc018', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb009', 'Cajamarca', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc019', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb010', 'Cusco', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc020', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb011', 'Huancavelica', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc021', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb012', 'Huanuco', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc022', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb013', 'Ica', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc023', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb014', 'Huancayo', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc024', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb015', 'Chiclayo', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc025', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb016', 'Maynas', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc026', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb017', 'Tambopata', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc027', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb018', 'Mariscal Nieto', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc028', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb019', 'Pasco', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc029', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb020', 'Piura', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc030', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb021', 'Puno', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc031', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb022', 'Moyobamba', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc032', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb023', 'Tacna', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc033', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb024', 'Tumbes', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc034', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb025', 'Coronel Portillo', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc035', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', 'Camana', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc036', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb002', 'Islay', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc037', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb003', 'Ascope', TRUE),
('cccccccc-cccc-cccc-cccc-ccccccccc038', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbb003', 'Pacasmayo', TRUE)
ON CONFLICT ("Id") DO UPDATE SET
"DepartmentId" = EXCLUDED."DepartmentId",
"Name" = EXCLUDED."Name",
"IsActive" = EXCLUDED."IsActive";

INSERT INTO "Districts" ("Id", "ProvinceId", "Name", "Ubigeo", "IsActive") VALUES
('dddddddd-dddd-dddd-dddd-ddddddddd001', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Miraflores', '150122', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd002', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'San Isidro', '150131', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd003', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Santiago de Surco', '150140', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd004', 'cccccccc-cccc-cccc-cccc-ccccccccc002', 'Cercado de Arequipa', '040101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd005', 'cccccccc-cccc-cccc-cccc-ccccccccc003', 'Trujillo', '130101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd006', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Jesus Maria', '150113', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd007', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Lince', '150116', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd008', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Magdalena del Mar', '150120', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd009', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Pueblo Libre', '150121', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd010', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'San Borja', '150130', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd011', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'San Miguel', '150136', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd012', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Surquillo', '150141', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd013', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'La Molina', '150114', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd014', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Barranco', '150104', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd015', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Chorrillos', '150108', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd016', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Los Olivos', '150117', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd017', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'San Juan de Lurigancho', '150132', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd018', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Ate', '150103', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd019', 'cccccccc-cccc-cccc-cccc-ccccccccc001', 'Comas', '150110', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd020', 'cccccccc-cccc-cccc-cccc-ccccccccc004', 'Callao', '070101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd021', 'cccccccc-cccc-cccc-cccc-ccccccccc004', 'Bellavista', '070102', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd022', 'cccccccc-cccc-cccc-cccc-ccccccccc004', 'La Punta', '070105', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd023', 'cccccccc-cccc-cccc-cccc-ccccccccc004', 'Ventanilla', '070106', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd024', 'cccccccc-cccc-cccc-cccc-ccccccccc005', 'Barranca', '150201', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd025', 'cccccccc-cccc-cccc-cccc-ccccccccc008', 'San Vicente de Canete', '150501', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd026', 'cccccccc-cccc-cccc-cccc-ccccccccc009', 'Huaral', '150601', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd027', 'cccccccc-cccc-cccc-cccc-ccccccccc011', 'Huacho', '150801', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd028', 'cccccccc-cccc-cccc-cccc-ccccccccc014', 'Chachapoyas', '010101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd029', 'cccccccc-cccc-cccc-cccc-ccccccccc015', 'Huaraz', '020101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd030', 'cccccccc-cccc-cccc-cccc-ccccccccc016', 'Abancay', '030101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd031', 'cccccccc-cccc-cccc-cccc-ccccccccc017', 'Ayacucho', '050101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd032', 'cccccccc-cccc-cccc-cccc-ccccccccc018', 'Cajamarca', '060101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd033', 'cccccccc-cccc-cccc-cccc-ccccccccc019', 'Cusco', '080101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd034', 'cccccccc-cccc-cccc-cccc-ccccccccc020', 'Huancavelica', '090101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd035', 'cccccccc-cccc-cccc-cccc-ccccccccc021', 'Huanuco', '100101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd036', 'cccccccc-cccc-cccc-cccc-ccccccccc022', 'Ica', '110101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd037', 'cccccccc-cccc-cccc-cccc-ccccccccc023', 'Huancayo', '120101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd038', 'cccccccc-cccc-cccc-cccc-ccccccccc024', 'Chiclayo', '140101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd039', 'cccccccc-cccc-cccc-cccc-ccccccccc025', 'Iquitos', '160101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd040', 'cccccccc-cccc-cccc-cccc-ccccccccc026', 'Tambopata', '170101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd041', 'cccccccc-cccc-cccc-cccc-ccccccccc027', 'Moquegua', '180101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd042', 'cccccccc-cccc-cccc-cccc-ccccccccc028', 'Chaupimarca', '190101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd043', 'cccccccc-cccc-cccc-cccc-ccccccccc029', 'Piura', '200101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd044', 'cccccccc-cccc-cccc-cccc-ccccccccc030', 'Puno', '210101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd045', 'cccccccc-cccc-cccc-cccc-ccccccccc031', 'Moyobamba', '220101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd046', 'cccccccc-cccc-cccc-cccc-ccccccccc032', 'Tacna', '230101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd047', 'cccccccc-cccc-cccc-cccc-ccccccccc033', 'Tumbes', '240101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd048', 'cccccccc-cccc-cccc-cccc-ccccccccc034', 'Calleria', '250101', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd049', 'cccccccc-cccc-cccc-cccc-ccccccccc035', 'Camana', '040201', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd050', 'cccccccc-cccc-cccc-cccc-ccccccccc036', 'Mollendo', '040701', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd051', 'cccccccc-cccc-cccc-cccc-ccccccccc037', 'Ascope', '130201', TRUE),
('dddddddd-dddd-dddd-dddd-ddddddddd052', 'cccccccc-cccc-cccc-cccc-ccccccccc038', 'San Pedro de Lloc', '130701', TRUE)
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

INSERT INTO "OlvaAgencies" ("Id", "Code", "Name", "Type", "Department", "Province", "District", "Address", "Phone", "ScheduleJson", "Latitude", "Longitude", "IsActive", "LastSyncedAt") VALUES
('81000000-0000-0000-0000-000000000001', 'OLVA-MIR-001', 'Olva Miraflores Larco', 'Agency', 'Lima', 'Lima', 'Miraflores', 'Av. Jose Larco 812, Miraflores', '01 7140909', '{"weekdays":"09:00-18:00","saturday":"09:00-13:00"}', -12.1221, -77.0294, TRUE, '2026-08-31T20:00:00Z'),
('81000000-0000-0000-0000-000000000002', 'OLVA-SIS-001', 'Olva San Isidro', 'Agency', 'Lima', 'Lima', 'San Isidro', 'Av. Camino Real 348, San Isidro', '01 7140909', '{"weekdays":"09:00-18:00","saturday":"09:00-13:00"}', -12.0989, -77.0367, TRUE, '2026-08-31T20:00:00Z'),
('81000000-0000-0000-0000-000000000003', 'OLVA-CAL-001', 'Olva Callao Central', 'Agency', 'Callao', 'Callao', 'Callao', 'Av. Saenz Pena 145, Callao', '01 7140909', '{"weekdays":"09:00-18:00","saturday":"09:00-13:00"}', -12.0609, -77.1330, TRUE, '2026-08-31T20:00:00Z'),
('81000000-0000-0000-0000-000000000004', 'OLVA-BEL-001', 'Olva Bellavista', 'Agency', 'Callao', 'Callao', 'Bellavista', 'Av. Oscar R. Benavides 3866, Bellavista', '01 7140909', '{"weekdays":"09:00-18:00","saturday":"09:00-13:00"}', -12.0621, -77.0882, TRUE, '2026-08-31T20:00:00Z'),
('81000000-0000-0000-0000-000000000005', 'OLVA-AQP-001', 'Olva Arequipa Cercado', 'Agency', 'Arequipa', 'Arequipa', 'Cercado de Arequipa', 'Calle Mercaderes 218, Arequipa', '054 714090', '{"weekdays":"09:00-18:00","saturday":"09:00-13:00"}', -16.3988, -71.5369, TRUE, '2026-08-31T20:00:00Z'),
('81000000-0000-0000-0000-000000000006', 'OLVA-TRU-001', 'Olva Trujillo Centro', 'Agency', 'La Libertad', 'Trujillo', 'Trujillo', 'Jr. Pizarro 560, Trujillo', '044 714090', '{"weekdays":"09:00-18:00","saturday":"09:00-13:00"}', -8.1116, -79.0288, TRUE, '2026-08-31T20:00:00Z')
ON CONFLICT ("Id") DO UPDATE SET
"Code" = EXCLUDED."Code",
"Name" = EXCLUDED."Name",
"Type" = EXCLUDED."Type",
"Department" = EXCLUDED."Department",
"Province" = EXCLUDED."Province",
"District" = EXCLUDED."District",
"Address" = EXCLUDED."Address",
"Phone" = EXCLUDED."Phone",
"ScheduleJson" = EXCLUDED."ScheduleJson",
"Latitude" = EXCLUDED."Latitude",
"Longitude" = EXCLUDED."Longitude",
"IsActive" = EXCLUDED."IsActive",
"LastSyncedAt" = EXCLUDED."LastSyncedAt";

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

INSERT INTO "AuditLogs" ("Id", "ActorUserId", "ActorRole", "Action", "EntityName", "EntityId", "OccurredAt", "IpAddress", "UserAgent", "OldValuesJson", "NewValuesJson", "Observation") VALUES
('ac000000-0000-0000-0000-000000000001', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Administrador', 'User.Login', 'User', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', '2026-08-31T20:01:00Z', '192.168.1.10', 'Mozilla/5.0 Mock Admin Browser', NULL, '{"value":"admin@sweetsilvia.local","deviceInfo":"Chrome Windows"}', 'Mock admin login'),
('ac000000-0000-0000-0000-000000000002', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Administrador', 'PATCH /api/users/aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2/role', 'Users', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', '2026-08-31T20:05:00Z', '192.168.1.10', 'Mozilla/5.0 Mock Admin Browser', NULL, '{"request":{"roleId":"33333333-3333-3333-3333-333333333333"}}', 'Administrador asigno rol de asistente'),
('ac000000-0000-0000-0000-000000000003', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'Asistente', 'PATCH /api/products/30000000-0000-0000-0000-000000000001', 'Products', '30000000-0000-0000-0000-000000000001', '2026-08-31T20:10:00Z', '192.168.1.11', 'Mozilla/5.0 Mock Assistant Browser', NULL, '{"request":{"name":"Polo Oversize Blanco","basePrice":89.90}}', 'Asistente actualizo un producto'),
('ac000000-0000-0000-0000-000000000004', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'Cliente', 'User.Login', 'User', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3', '2026-08-31T20:12:00Z', '192.168.1.20', 'Mozilla/5.0 Mock Customer Mobile', NULL, '{"value":"cliente@sweetsilvia.local","deviceInfo":"Mobile Android"}', 'Mock customer login')
ON CONFLICT ("Id") DO UPDATE SET
"ActorUserId" = EXCLUDED."ActorUserId",
"ActorRole" = EXCLUDED."ActorRole",
"Action" = EXCLUDED."Action",
"EntityName" = EXCLUDED."EntityName",
"EntityId" = EXCLUDED."EntityId",
"OccurredAt" = EXCLUDED."OccurredAt",
"IpAddress" = EXCLUDED."IpAddress",
"UserAgent" = EXCLUDED."UserAgent",
"OldValuesJson" = EXCLUDED."OldValuesJson",
"NewValuesJson" = EXCLUDED."NewValuesJson",
"Observation" = EXCLUDED."Observation";

COMMIT;
