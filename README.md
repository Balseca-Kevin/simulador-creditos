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
| Persistencia | PostgreSQL 17 | `authdb` (identidades) y `creditdb` (tasas e historial) |

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
             PostgreSQL 17
```

## Reglas de negocio

El tipo de crédito determina la tasa de interés anual y la prima del seguro de
desgravamen aplicadas:

| Tipo de crédito | Tasa anual | Desgravamen mensual | Descripción |
|---|---|---|---|
| Crédito de Consumo | 15.50 % | 0.05 % del saldo | Adquisición de bienes de consumo o pago de servicios |
| Crédito Inmobiliario | 8.50 % | 0.04 % del saldo | Compra, construcción o remodelación de vivienda |
| Microcrédito | 22.00 % | 0.07 % del saldo | Financiamiento para actividades productivas a pequeña escala |

Las tasas se almacenan en `creditdb`, no están fijadas en el código: la regla es
dinámica y puede actualizarse sin recompilar los servicios. Las primas de
desgravamen son valores referenciales del mercado ecuatoriano.

### Métodos de amortización

- **Francés:** cuota fija; el interés decrece y el capital crece con el tiempo.
- **Alemán:** amortización de capital fija; la cuota total decrece período a período.

### Frecuencia de pago

Mensual, bimensual, trimestral, semestral o al vencimiento (un único pago). La
tasa del período es la anual repartida según los meses que cubre cada cuota
(12 % anual = 3 % trimestral), y el plazo debe ser múltiplo de esa frecuencia.

### Seguro de desgravamen

Opcional. Se cobra en cada cuota sobre el saldo adeudado al inicio del período,
por lo que disminuye a medida que se amortiza el crédito.

### Ingreso mínimo requerido

La cuota más alta de la tabla, llevada a su equivalente mensual, dividida para
0.40: la cuota no debe superar el **40 % del ingreso**. Se redondea hacia arriba
al centavo para no quedar nunca por debajo del umbral.

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

Crea también `backend/Credit.Api/appsettings.Development.json`:

```jsonc
{
  "ConnectionStrings": {
    "CreditDb": "Host=localhost;Port=5432;Database=creditdb;Username=postgres;Password=TU_CONTRASEÑA"
  },
  "Jwt": {
    "Issuer": "SimuladorCreditos.AuthApi",
    "Audience": "SimuladorCreditos.Clientes",
    "Key": "LA_MISMA_CLAVE_QUE_EN_AUTH_API"
  },
  "Cors": { "OrigenesPermitidos": [ "http://localhost:5173" ] }
}
```

> **Importante:** `Jwt.Key`, `Issuer` y `Audience` deben ser idénticos en ambos
> servicios. La Credit API valida la firma de los tokens que emite la Auth API; si
> las claves difieren, rechazará con 401 incluso a usuarios con sesión válida.

No hace falta crear las bases a mano: al arrancar en modo desarrollo, EF Core crea
`authdb` y `creditdb`, aplica las migraciones y siembra el catálogo de tasas.

### 2. Levantar los microservicios

En dos terminales:

```bash
cd backend/Auth.Api
dotnet run --launch-profile http     # http://localhost:5080
```

```bash
cd backend/Credit.Api
dotnet run --launch-profile http     # http://localhost:5090
```

### 3. Ejecutar las pruebas

```bash
cd backend
dotnet test SimuladorCreditos.slnx   # 66 pruebas del motor de amortización
```

### 4. Levantar el frontend

```bash
cd frontend
npm install
npm run dev                          # http://localhost:5173
```

Abre `http://localhost:5173`, regístrate y accede al simulador.
