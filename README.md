# Simulador de Créditos

Sistema de simulación de créditos con arquitectura de microservicios que permite a un
usuario autenticarse y generar tablas de amortización comparativas por los métodos
**Francés** y **Alemán**.

- **Desarrollador:** Kevin David Balseca Tumbaco
- **Institución:** Universidad Técnica de Ambato — Ambato, Ecuador
- **Asignatura:** Metodologías de Desarrollo de Software
- **Inicio:** 11 de septiembre de 2026 · **Duración:** 15 días (3 sprints de 5 días)

---

## Arquitectura

| Componente | Tecnología | Responsabilidad |
|---|---|---|
| Frontend | React 19 + Vite + TypeScript + Tailwind CSS v4 | SPA: login, formulario de simulación y render de tablas |
| Auth API | .NET 10 (ASP.NET Core) | Registro, login y emisión de tokens JWT |
| Credit API | .NET 10 (ASP.NET Core) | Motor de cálculo y reglas de amortización (protegido por JWT) |
| Persistencia | PostgreSQL 18 | `authdb` (identidades) y `creditdb` (tasas e historial) |

Se aplica el patrón **Database per Service**: cada microservicio es dueño exclusivo de
su base de datos y ningún servicio consulta las tablas del otro.

```
          ┌─────────────────┐
          │  React SPA      │
          └────┬───────┬────┘
      JWT      │       │   Bearer JWT
               ▼       ▼
     ┌──────────────┐ ┌──────────────┐
     │  Auth API    │ │  Credit API  │
     └──────┬───────┘ └──────┬───────┘
            ▼                ▼
        [ authdb ]      [ creditdb ]
             PostgreSQL 18
```

## Reglas de negocio

El tipo de crédito determina la tasa de interés anual aplicada:

| Tipo de crédito | Tasa referencial anual | Descripción |
|---|---|---|
| Crédito de Consumo | 15.50 % | Adquisición de bienes de consumo o pago de servicios |
| Crédito Inmobiliario | 8.50 % | Compra, construcción o remodelación de vivienda |
| Microcrédito | 22.00 % | Financiamiento para actividades productivas a pequeña escala |

Las tasas se almacenan en `creditdb`, no están fijadas en el código: la regla es
dinámica y puede actualizarse sin recompilar los servicios.

### Métodos de amortización

- **Francés:** cuota mensual fija; el interés decrece y el capital crece con el tiempo.
- **Alemán:** amortización de capital fija; la cuota total decrece mes a mes.

## Estructura del repositorio

```
proyecto_SimuladorDeCreditos/
├── backend/          Solución .NET (Auth.Api, Credit.Api, pruebas)
├── frontend/         SPA en React + Vite
├── docs/             Documento oficial, backlog y evidencias de sprint
└── README.md
```

## Metodología

Desarrollo iterativo bajo el **Manifiesto Ágil**, con una adaptación de Scrum en
tres micro-sprints de 5 días. El detalle del backlog, las historias de usuario y la
evidencia de cada incremento está en [docs/SPRINTS.md](docs/SPRINTS.md).

## Puesta en marcha

### Requisitos

- .NET SDK 10
- Node.js 22 o superior
- PostgreSQL 15 o superior, escuchando en `localhost:5432`

### 1. Configurar las credenciales locales

El archivo `backend/Auth.Api/appsettings.Development.json` está excluido del
control de versiones porque contiene la contraseña de la base y la clave de firma
de los tokens. Créalo a partir de esta plantilla:

```jsonc
{
  "ConnectionStrings": {
    "AuthDb": "Host=localhost;Port=5432;Database=authdb;Username=postgres;Password=TU_CONTRASEÑA"
  },
  "Jwt": {
    "Issuer": "SimuladorCreditos.AuthApi",
    "Audience": "SimuladorCreditos.Clientes",
    "Key": "UNA_CLAVE_ALEATORIA_DE_AL_MENOS_32_CARACTERES",
    "MinutosDeVigencia": 120
  },
  "Cors": { "OrigenesPermitidos": [ "http://localhost:5173" ] }
}
```

No hace falta crear la base a mano: al arrancar en modo desarrollo, EF Core crea
`authdb` y aplica las migraciones pendientes.

### 2. Levantar la Auth API

```bash
cd backend/Auth.Api
dotnet run --launch-profile http     # http://localhost:5080
```

### 3. Levantar el frontend

```bash
cd frontend
npm install
npm run dev                          # http://localhost:5173
```

Abre `http://localhost:5173`, regístrate y accede al simulador.
