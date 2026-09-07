# Sweet Silvia Dev Mocks

Esta carpeta contiene datos mock para pruebas locales del backend y frontend.

Puedes ignorar toda la carpeta en Git cuando ya no quieras versionar datos de prueba:

```gitignore
DevMocks/
```

## Usuarios

| Rol | Email | Password |
| --- | --- | --- |
| Administrador | admin@sweetsilvia.local | Admin123* |
| Asistente | asistente@sweetsilvia.local | Asistente123* |
| Cliente | cliente@sweetsilvia.local | Cliente123* |

## Auditoria y sesiones

El seed incluye refresh tokens mock con `IpAddress`, `UserAgent` y `LastUsedAt`, ademas de movimientos de auditoria para probar:

- `GET /api/users/me/refresh-tokens`
- `GET /api/users/{id}/refresh-tokens`
- `GET /api/audit-logs`

## Ubigeo de prueba

El seed incluye 25 departamentos, 38 provincias y 52 distritos. Para Lima se agregaron varias provincias y distritos comunes, suficiente para probar combos dependientes y calculo de envios locales/provincia.

## Ejecutar seed

Desde la raiz de la solucion:

```powershell
psql "Host=localhost;Port=5433;Database=sweet_silvia;Username=postgres;Password=postgres" -f DevMocks/seed-dev.sql
```

Si usas Docker, la base configurada en `docker-compose.yml` se llama `sweet_silvia`:

```powershell
docker cp DevMocks/seed-dev.sql sweet-silvia-db:/tmp/seed-dev.sql
docker exec sweet-silvia-db psql -U postgres -d sweet_silvia -f /tmp/seed-dev.sql
```

## Limpiar mocks

```powershell
psql "Host=localhost;Port=5433;Database=sweet_silvia;Username=postgres;Password=postgres" -f DevMocks/reset-dev.sql
```
