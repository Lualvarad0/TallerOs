# TallerOS

SaaS multi-empresa para talleres mecánicos (autos y motos): órdenes de trabajo tipo Jira, tiempo por mecánico, jornada laboral, inventario con margen, hallazgos y avisos a clientes.

> **Este repositorio es la app de computadora** (web + escritorio). El empaquetado móvil (Android/iOS) vive en un proyecto separado, aparte de este monorepo.

## Stack

- **Frontend**: Nuxt 3 en modo SPA (`ssr: false`), TypeScript estricto, Tailwind CSS, Pinia, Zod + VeeValidate. Empaquetado de escritorio con Tauri.
- **Backend**: .NET 10, ASP.NET Core Web API, Clean Architecture (Domain / Application / Infrastructure / Api), CQRS, FluentValidation, EF Core.
- **Base de datos**: PostgreSQL (Docker), con Row-Level Security por empresa.
- **Caché y tiempo real**: Redis, SignalR (Kanban y alertas), Hangfire (tareas programadas). Archivos en almacenamiento compatible S3 (Garage en local).

## Estructura

```text
TallerOS/
  docs/                  especificación, modelo de datos, diseño y convenciones
  apps/
    taller/              app Nuxt principal (taller)
    mi-garage/            app Nuxt del cliente final (más adelante)
  packages/
    ui/                  design system: tokens y componentes base
    validators/          cédula, RUC, teléfono, VIN, placa (TypeScript)
    api-client/          cliente tipado generado desde OpenAPI
  backend/
    src/
      TallerOS.Domain/          entidades, value objects, eventos y reglas de dominio
      TallerOS.Application/     casos de uso por módulo (Commands / Queries / DTOs / Validators)
      TallerOS.Infrastructure/  EF Core, migraciones, Redis, S3, correo, WhatsApp, Hangfire
      TallerOS.Api/             endpoints por módulo, middlewares, autenticación, rate limiting
    tests/                      pruebas unitarias, de integración y de aislamiento entre empresas
  infra/
    docker-compose.yml
    postgres/init/       scripts de roles, extensiones y RLS
    seed/                datos semilla (catálogos, permisos, empresa demo)
```

Más detalle de arquitectura, convenciones de base de datos, seguridad y patrones de diseño usados en [`docs/convenciones.md`](docs/convenciones.md). Especificación funcional completa en [`docs/especificacion.md`](docs/especificacion.md).

## Módulos (orden de construcción)

1. Organización y seguridad (empresa, sucursal, área, usuario, personal, roles, permisos, autenticación)
2. Catálogos de vehículos (tipo de vehículo, marca, modelo) y validadores
3. Clientes y vehículos (cliente, vehículo, placas, relación cliente-vehículo)
4. Órdenes de trabajo y Kanban (OT, estados, subtareas, recepción)
5. Tiempos y jornada (registro de tiempo, modos, marcación, horas extra)
6. Hallazgos y cotizaciones
7. Inventario, compras y margen
8. Avisos al cliente (correo, WhatsApp, recordatorios)

## Puesta en marcha

```powershell
# Infraestructura (PostgreSQL, Redis, almacenamiento S3 local)
docker compose -f infra/docker-compose.yml up -d

# Backend
cd backend
dotnet run --project src/TallerOS.Api

# Frontend
cd apps/taller
npm install
npm run dev
```

Variables de entorno de ejemplo en [`.env.example`](.env.example).

## Seguridad y multi-empresa

- Aislamiento por empresa reforzado en dos capas: filtro global de EF Core + Row-Level Security en PostgreSQL (`FORCE ROW LEVEL SECURITY`).
- Autenticación con ASP.NET Identity, JWT de corta duración y refresh token rotativo en cookie `HttpOnly`.
- Autorización por permiso, no por nombre de rol.
- Cada endpoint nuevo tiene una prueba que confirma que una empresa no puede leer ni escribir datos de otra.
