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

## Ejecutar seed

Desde la raiz de la solucion:

```powershell
psql "Host=localhost;Port=5432;Database=sweet_silvia_dev;Username=postgres;Password=postgres" -f DevMocks/seed-dev.sql
```

Si usas Docker, la base configurada en `docker-compose.yml` se llama `sweet_silvia`:

```powershell
docker cp DevMocks/seed-dev.sql sweet-silvia-db:/tmp/seed-dev.sql
docker exec sweet-silvia-db psql -U postgres -d sweet_silvia -f /tmp/seed-dev.sql
```

## Limpiar mocks

```powershell
psql "Host=localhost;Port=5432;Database=sweet_silvia_dev;Username=postgres;Password=postgres" -f DevMocks/reset-dev.sql
```
