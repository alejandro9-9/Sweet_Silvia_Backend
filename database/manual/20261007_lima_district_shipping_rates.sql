-- Carga las ubicaciones faltantes y sus tarifas distritales de Lima/Callao.
-- No crea tablas. Primero completa Districts; despues actualiza ShippingRates.
-- Crea Delivery Lima / OwnDelivery si falta. Respeta el tipo real del departamento.
-- Los importes son PEN; el checkout puede agregar los recargos de peso/volumen.
-- Ejecutar en una transaccion; cualquier inconsistencia aborta la carga completa.

BEGIN;

-- Fase 1: garantizar departamentos, provincias y distritos existentes.
-- Los departamentos/provincias deben existir; no se duplican ni se crean aqui.
DO $$
DECLARE
    item record;
    department_id uuid;
    province_id uuid;
    district_id uuid;
    global_ubigeo_matches integer;
    global_ubigeo_province_id uuid;
    matches integer;
BEGIN
    FOR item IN
        SELECT * FROM (VALUES
            ('070104', 'Callao', 'Callao', 'La Perla', 15.00::numeric),
            ('070101', 'Callao', 'Callao', 'Callao', 15.00::numeric),
            ('070102', 'Callao', 'Callao', 'Bellavista', 15.00::numeric),
            ('070103', 'Callao', 'Callao', 'Carmen de la Legua Reynoso', 15.00::numeric),
            ('150136', 'Lima', 'Lima', 'San Miguel', 15.00::numeric),
            ('150120', 'Lima', 'Lima', 'Magdalena del Mar', 15.00::numeric),
            ('150121', 'Lima', 'Lima', 'Pueblo Libre', 15.00::numeric),
            ('150113', 'Lima', 'Lima', 'Jesús María', 15.00::numeric),
            ('150105', 'Lima', 'Lima', 'Breña', 15.00::numeric),
            ('150101', 'Lima', 'Lima', 'Lima', 15.00::numeric),
            ('150131', 'Lima', 'Lima', 'San Isidro', 20.00::numeric),
            ('150116', 'Lima', 'Lima', 'Lince', 15.00::numeric),
            ('150122', 'Lima', 'Lima', 'Miraflores', 20.00::numeric),
            ('150104', 'Lima', 'Lima', 'Barranco', 20.00::numeric),
            ('150134', 'Lima', 'Lima', 'San Luis', 20.00::numeric),
            ('150115', 'Lima', 'Lima', 'La Victoria', 20.00::numeric),
            ('150130', 'Lima', 'Lima', 'San Borja', 20.00::numeric),
            ('150141', 'Lima', 'Lima', 'Surquillo', 20.00::numeric),
            ('150140', 'Lima', 'Lima', 'Santiago de Surco', 20.00::numeric),
            ('150128', 'Lima', 'Lima', 'Rímac', 20.00::numeric),
            ('150108', 'Lima', 'Lima', 'Chorrillos', 20.00::numeric),
            ('150135', 'Lima', 'Lima', 'San Martín de Porres', 20.00::numeric),
            ('150117', 'Lima', 'Lima', 'Los Olivos', 20.00::numeric),
            ('150112', 'Lima', 'Lima', 'Independencia', 25.00::numeric),
            ('150110', 'Lima', 'Lima', 'Comas', 25.00::numeric),
            ('150125', 'Lima', 'Lima', 'Puente Piedra', 25.00::numeric),
            ('150106', 'Lima', 'Lima', 'Carabayllo', 25.00::numeric),
            ('150102', 'Lima', 'Lima', 'Ancón', 25.00::numeric),
            ('150139', 'Lima', 'Lima', 'Santa Rosa', 20.00::numeric),
            ('150111', 'Lima', 'Lima', 'El Agustino', 20.00::numeric),
            ('150132', 'Lima', 'Lima', 'San Juan de Lurigancho', 20.00::numeric),
            ('150137', 'Lima', 'Lima', 'Santa Anita', 20.00::numeric),
            ('150103', 'Lima', 'Lima', 'Ate', 20.00::numeric),
            ('150114', 'Lima', 'Lima', 'La Molina', 20.00::numeric),
            ('150118', 'Lima', 'Lima', 'Lurigancho', 20.00::numeric),
            ('150107', 'Lima', 'Lima', 'Chaclacayo', 20.00::numeric),
            ('150109', 'Lima', 'Lima', 'Cieneguilla', 20.00::numeric),
            ('150133', 'Lima', 'Lima', 'San Juan de Miraflores', 20.00::numeric),
            ('150143', 'Lima', 'Lima', 'Villa María del Triunfo', 20.00::numeric),
            ('150142', 'Lima', 'Lima', 'Villa El Salvador', 20.00::numeric),
            ('150119', 'Lima', 'Lima', 'Lurín', 20.00::numeric),
            ('150123', 'Lima', 'Lima', 'Pachacámac', 20.00::numeric),
            ('150124', 'Lima', 'Lima', 'Pucusana', 20.00::numeric),
            ('150126', 'Lima', 'Lima', 'Punta Hermosa', 20.00::numeric),
            ('150127', 'Lima', 'Lima', 'Punta Negra', 20.00::numeric),
            ('150129', 'Lima', 'Lima', 'San Bartolo', 20.00::numeric),
            ('150138', 'Lima', 'Lima', 'Santa María del Mar', 20.00::numeric)
        ) AS requested("Ubigeo", "DepartmentName", "ProvinceName", "DistrictName", "CostPEN")
    LOOP
        SELECT count(*), (array_agg(d."Id"))[1]
        INTO matches, department_id
        FROM "Departments" d
        WHERE d."IsActive"
          AND lower(btrim(d."Name")) = lower(item."DepartmentName");
        IF matches <> 1 THEN
            RAISE EXCEPTION 'Se esperaba un departamento activo %, encontrados: %.', item."DepartmentName", matches;
        END IF;

        SELECT count(*), (array_agg(p."Id"))[1]
        INTO matches, province_id
        FROM "Provinces" p
        WHERE p."DepartmentId" = department_id
          AND p."IsActive"
          AND lower(btrim(p."Name")) = lower(item."ProvinceName");
        IF matches <> 1 THEN
            RAISE EXCEPTION 'Se esperaba una provincia activa % en %, encontradas: %.', item."ProvinceName", item."DepartmentName", matches;
        END IF;

        SELECT count(*), (array_agg(d."ProvinceId"))[1]
        INTO global_ubigeo_matches, global_ubigeo_province_id
        FROM "Districts" d
        WHERE d."Ubigeo" = item."Ubigeo";
        IF global_ubigeo_matches > 1
           OR (global_ubigeo_matches = 1 AND global_ubigeo_province_id <> province_id) THEN
            RAISE EXCEPTION 'El Ubigeo % ya existe duplicado o asociado a otra provincia.', item."Ubigeo";
        END IF;

        SELECT count(*), (array_agg(d."Id"))[1]
        INTO matches, district_id
        FROM "Districts" d
        WHERE d."ProvinceId" = province_id
          AND (
              d."Ubigeo" = item."Ubigeo"
              OR lower(regexp_replace(translate(btrim(d."Name"), 'áéíóúüñÁÉÍÓÚÜÑ', 'aeiouunAEIOUUN'), '[^a-zA-Z0-9]', '', 'g'))
                 = lower(regexp_replace(translate(btrim(item."DistrictName"), 'áéíóúüñÁÉÍÓÚÜÑ', 'aeiouunAEIOUUN'), '[^a-zA-Z0-9]', '', 'g'))
          );
        IF matches > 1 THEN
            RAISE EXCEPTION 'Ubigeo/nombre ambiguo para distrito % (%): se encontraron % filas.', item."DistrictName", item."Ubigeo", matches;
        ELSIF matches = 0 THEN
            INSERT INTO "Districts" ("Id", "ProvinceId", "Name", "Ubigeo", "IsActive")
            VALUES (gen_random_uuid(), province_id, item."DistrictName", item."Ubigeo", TRUE)
            RETURNING "Id" INTO district_id;
        ELSE
            UPDATE "Districts"
            SET "Name" = item."DistrictName",
                "Ubigeo" = item."Ubigeo",
                "IsActive" = TRUE
            WHERE "Id" = district_id;
        END IF;
    END LOOP;
END $$;

-- Fase 2: crear/actualizar las tarifas usando los DistrictId ya garantizados.
DO $$
DECLARE
    item record;
    department_id uuid;
    province_id uuid;
    district_id uuid;
    destination_type text;
    courier_id uuid;
    rate_id uuid;
    matches integer;
BEGIN
    SELECT count(*), (array_agg(c."Id"))[1]
    INTO matches, courier_id
    FROM "Couriers" c
    WHERE c."Name" = 'Delivery Lima'
      AND c."Type" = 'OwnDelivery'
      AND c."IsActive";

    IF matches = 0 THEN
        SELECT count(*) INTO matches
        FROM "Couriers" c
        WHERE c."Name" = 'Delivery Lima'
          AND c."Type" = 'OwnDelivery';
        IF matches > 0 THEN
            RAISE EXCEPTION 'Existe un courier Delivery Lima (OwnDelivery) inactivo. Revisalo manualmente antes de continuar.';
        END IF;

        INSERT INTO "Couriers" ("Id", "Name", "Type", "RequiresExternalIntegration", "IsActive")
        VALUES (gen_random_uuid(), 'Delivery Lima', 'OwnDelivery', FALSE, TRUE)
        RETURNING "Id" INTO courier_id;
    ELSIF matches > 1 THEN
        RAISE EXCEPTION 'Hay % couriers activos Delivery Lima (OwnDelivery); debe existir como maximo uno.', matches;
    END IF;

    SELECT count(*), (array_agg(c."Id"))[1]
    INTO matches, courier_id
    FROM "Couriers" c
    WHERE c."Name" = 'Delivery Lima'
      AND c."Type" = 'OwnDelivery'
      AND c."IsActive";
    IF matches <> 1 THEN
        RAISE EXCEPTION 'Se esperaba exactamente un courier activo Delivery Lima (OwnDelivery), encontrados: %.', matches;
    END IF;

    FOR item IN
        SELECT * FROM (VALUES
            ('070104', 'Callao', 'Callao', 'La Perla', 15.00::numeric),
            ('070101', 'Callao', 'Callao', 'Callao', 15.00::numeric),
            ('070102', 'Callao', 'Callao', 'Bellavista', 15.00::numeric),
            ('070103', 'Callao', 'Callao', 'Carmen de la Legua Reynoso', 15.00::numeric),
            ('150136', 'Lima', 'Lima', 'San Miguel', 15.00::numeric),
            ('150120', 'Lima', 'Lima', 'Magdalena del Mar', 15.00::numeric),
            ('150121', 'Lima', 'Lima', 'Pueblo Libre', 15.00::numeric),
            ('150113', 'Lima', 'Lima', 'Jesús María', 15.00::numeric),
            ('150105', 'Lima', 'Lima', 'Breña', 15.00::numeric),
            ('150101', 'Lima', 'Lima', 'Lima', 15.00::numeric),
            ('150131', 'Lima', 'Lima', 'San Isidro', 20.00::numeric),
            ('150116', 'Lima', 'Lima', 'Lince', 15.00::numeric),
            ('150122', 'Lima', 'Lima', 'Miraflores', 20.00::numeric),
            ('150104', 'Lima', 'Lima', 'Barranco', 20.00::numeric),
            ('150134', 'Lima', 'Lima', 'San Luis', 20.00::numeric),
            ('150115', 'Lima', 'Lima', 'La Victoria', 20.00::numeric),
            ('150130', 'Lima', 'Lima', 'San Borja', 20.00::numeric),
            ('150141', 'Lima', 'Lima', 'Surquillo', 20.00::numeric),
            ('150140', 'Lima', 'Lima', 'Santiago de Surco', 20.00::numeric),
            ('150128', 'Lima', 'Lima', 'Rímac', 20.00::numeric),
            ('150108', 'Lima', 'Lima', 'Chorrillos', 20.00::numeric),
            ('150135', 'Lima', 'Lima', 'San Martín de Porres', 20.00::numeric),
            ('150117', 'Lima', 'Lima', 'Los Olivos', 20.00::numeric),
            ('150112', 'Lima', 'Lima', 'Independencia', 25.00::numeric),
            ('150110', 'Lima', 'Lima', 'Comas', 25.00::numeric),
            ('150125', 'Lima', 'Lima', 'Puente Piedra', 25.00::numeric),
            ('150106', 'Lima', 'Lima', 'Carabayllo', 25.00::numeric),
            ('150102', 'Lima', 'Lima', 'Ancón', 25.00::numeric),
            ('150139', 'Lima', 'Lima', 'Santa Rosa', 20.00::numeric),
            ('150111', 'Lima', 'Lima', 'El Agustino', 20.00::numeric),
            ('150132', 'Lima', 'Lima', 'San Juan de Lurigancho', 20.00::numeric),
            ('150137', 'Lima', 'Lima', 'Santa Anita', 20.00::numeric),
            ('150103', 'Lima', 'Lima', 'Ate', 20.00::numeric),
            ('150114', 'Lima', 'Lima', 'La Molina', 20.00::numeric),
            ('150118', 'Lima', 'Lima', 'Lurigancho', 20.00::numeric),
            ('150107', 'Lima', 'Lima', 'Chaclacayo', 20.00::numeric),
            ('150109', 'Lima', 'Lima', 'Cieneguilla', 20.00::numeric),
            ('150133', 'Lima', 'Lima', 'San Juan de Miraflores', 20.00::numeric),
            ('150143', 'Lima', 'Lima', 'Villa María del Triunfo', 20.00::numeric),
            ('150142', 'Lima', 'Lima', 'Villa El Salvador', 20.00::numeric),
            ('150119', 'Lima', 'Lima', 'Lurín', 20.00::numeric),
            ('150123', 'Lima', 'Lima', 'Pachacámac', 20.00::numeric),
            ('150124', 'Lima', 'Lima', 'Pucusana', 20.00::numeric),
            ('150126', 'Lima', 'Lima', 'Punta Hermosa', 20.00::numeric),
            ('150127', 'Lima', 'Lima', 'Punta Negra', 20.00::numeric),
            ('150129', 'Lima', 'Lima', 'San Bartolo', 20.00::numeric),
            ('150138', 'Lima', 'Lima', 'Santa María del Mar', 20.00::numeric)
        ) AS requested("Ubigeo", "DepartmentName", "ProvinceName", "DistrictName", "CostPEN")
    LOOP
        SELECT d."Id", p."Id"
        INTO district_id, province_id
        FROM "Districts" d
        JOIN "Provinces" p ON p."Id" = d."ProvinceId"
        JOIN "Departments" dep ON dep."Id" = p."DepartmentId"
        WHERE d."Ubigeo" = item."Ubigeo"
          AND d."Name" = item."DistrictName"
          AND d."IsActive" AND p."IsActive" AND dep."IsActive"
          AND p."Name" = item."ProvinceName"
          AND dep."Name" = item."DepartmentName";
        IF NOT FOUND THEN
            RAISE EXCEPTION 'No se encuentra activo el distrito % (%), luego de la fase de ubicaciones.', item."DistrictName", item."Ubigeo";
        END IF;
        SELECT p."DepartmentId", dep."DestinationType"
        INTO department_id, destination_type
        FROM "Provinces" p
        JOIN "Departments" dep ON dep."Id" = p."DepartmentId"
        WHERE p."Id" = province_id;

        SELECT count(*) INTO matches
        FROM "ShippingRates" r
        WHERE r."DistrictId" = district_id
          AND r."IsActive"
          AND NOT (
              r."CourierId" = courier_id
              AND r."CourierType" = 'OwnDelivery'
              AND r."ServiceType" = 'Regular'
          );
        IF matches > 0 THEN
            RAISE EXCEPTION 'El distrito % ya tiene % tarifa(s) activa(s) de otro courier/servicio. Resuelve el conflicto antes de cargar.', item."DistrictName", matches;
        END IF;

        SELECT count(*), (array_agg(r."Id"))[1]
        INTO matches, rate_id
        FROM "ShippingRates" r
        WHERE r."DistrictId" = district_id
          AND r."CourierId" = courier_id
          AND r."CourierType" = 'OwnDelivery'
          AND r."ServiceType" = 'Regular';
        IF matches > 1 THEN
            RAISE EXCEPTION 'Hay tarifas repetidas de Delivery Lima / Regular para el distrito %.', item."DistrictName";
        ELSIF matches = 1 THEN
            UPDATE "ShippingRates"
            SET "DepartmentId" = department_id,
                "ProvinceId" = province_id,
                "DistrictId" = district_id,
                "DestinationType" = destination_type,
                "Country" = 'PE',
                "Cost" = (item."CostPEN"::numeric(12, 2))::text || '|PEN',
                "FreeShippingMinimumAmount" = NULL,
                "EstimatedTime" = NULL,
                "IsActive" = TRUE,
                "UpdatedAt" = now()
            WHERE "Id" = rate_id;
        ELSE
            INSERT INTO "ShippingRates" (
                "Id", "CourierId", "CourierType", "Country", "DepartmentId", "ProvinceId", "DistrictId",
                "DestinationType", "ServiceType", "Cost", "FreeShippingMinimumAmount", "EstimatedTime",
                "IsActive", "CreatedAt", "UpdatedAt"
            ) VALUES (
                gen_random_uuid(), courier_id, 'OwnDelivery', 'PE', department_id, province_id, district_id,
                destination_type, 'Regular', (item."CostPEN"::numeric(12, 2))::text || '|PEN', NULL, NULL,
                TRUE, now(), NULL
            );
        END IF;
    END LOOP;
END $$;

COMMIT;

-- Verificacion posterior: debe devolver 47 filas, una por distrito.
SELECT d."Ubigeo", d."Name" AS "Distrito", r."Cost" AS "Tarifa", r."ServiceType" AS "Servicio"
FROM "Districts" d
JOIN "Provinces" p ON p."Id" = d."ProvinceId"
JOIN "Departments" dep ON dep."Id" = p."DepartmentId"
JOIN "ShippingRates" r ON r."DistrictId" = d."Id"
JOIN "Couriers" c ON c."Id" = r."CourierId"
WHERE d."Ubigeo" IN (
    '070104','070101','070102','070103','150136','150120','150121','150113','150105','150101',
    '150131','150116','150122','150104','150134','150115','150130','150141','150140','150128',
    '150108','150135','150117','150112','150110','150125','150106','150102','150139','150111',
    '150132','150137','150103','150114','150118','150107','150109','150133','150143','150142',
    '150119','150123','150124','150126','150127','150129','150138'
)
  AND d."IsActive" AND p."IsActive" AND dep."IsActive"
  AND r."IsActive" AND r."DestinationType" = dep."DestinationType"
  AND r."ServiceType" = 'Regular'
  AND c."Name" = 'Delivery Lima' AND c."Type" = 'OwnDelivery'
ORDER BY d."Ubigeo";
