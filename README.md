# Simulador de Créditos

Sistema de simulación de créditos con arquitectura de microservicios que permite a un
usuario autenticarse y generar tablas de amortización comparativas por los métodos
**Francés** y **Alemán**.

- **Desarrollador:** Kevin David Balseca Tumbaco
- **Institución:** Universidad Técnica de Ambato — Ambato, Ecuador
- **Asignatura:** Metodologías Ágiles
- **Inicio:** 11 de septiembre de 2026 · **Duración:** 15 días (3 sprints de 5 días)

---

## Arquitectura

| Componente | Puerto | Tecnología | Responsabilidad |
|---|---|---|---|
| `creditos-web` | 5173 | React 19 + Vite + TypeScript + Tailwind CSS v4 | SPA: login, formulario de simulación y tablas |
| `ApiGateway` | 5000 | .NET 10 + YARP | Punto de entrada único; enruta hacia cada servicio |
| `AuthService` | 5080 | .NET 10 (ASP.NET Core) | Registro, login y emisión de tokens JWT |
| `CreditService` | 5090 | .NET 10 (ASP.NET Core) | Motor de cálculo y reglas de amortización (protegido por JWT) |
| Persistencia | 5432 | PostgreSQL 17 | `authdb` (identidades) y `creditdb` (tasas e historial) |

Se aplica el patrón **Database per Service**: cada microservicio es dueño exclusivo de
su base de datos y ningún servicio consulta las tablas del otro.

La SPA habla con un solo origen, el gateway, y no conoce los puertos de los
servicios. El gateway **no valida el token**: lo reenvía y cada servicio decide,
para no repetir la clave de firma en tres lugares.

```
                ┌──────────────────┐
                │  creditos-web    │  :5173
                └────────┬─────────┘
                         │  Bearer JWT
                         ▼
                ┌──────────────────┐
                │   ApiGateway     │  :5000
                └───┬──────────┬───┘
         /api/auth  │          │  /api/creditos
                    ▼          ▼
        ┌───────────────┐ ┌───────────────┐
        │  AuthService  │ │ CreditService │
        │     :5080     │ │     :5090     │
        └───────┬───────┘ └───────┬───────┘
                ▼                 ▼
           [ authdb ]        [ creditdb ]
                  PostgreSQL 17  :5432
```

### Capas de cada microservicio

| Carpeta | Contiene | Depende de |
|---|---|---|
| `Dominio/` | Entidades y reglas propias del negocio | nada |
| `Aplicacion/` | Casos de uso, DTOs y servicios (motor de amortización, emisión de tokens) | `Dominio` |
| `Estructura/` | Acceso a datos: contextos de EF Core y su configuración | `Dominio` |
| `Presentacion/` | Controladores y formato de las respuestas HTTP | `Aplicacion` |
| `Migrations/` | Migraciones de EF Core | `Estructura` |

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
│
├── backend/
│   │
│   ├── AuthService/
│   │   ├── Dominio/
│   │   ├── Aplicacion/
│   │   ├── Estructura/
│   │   ├── Presentacion/
│   │   ├── Migrations/
│   │   └── Program.cs
│   │
│   ├── CreditService/
│   │   ├── Dominio/
│   │   ├── Aplicacion/
│   │   ├── Estructura/
│   │   ├── Presentacion/
│   │   ├── Migrations/
│   │   └── Program.cs
│   │
│   ├── CreditService.Tests/      66 pruebas del motor de amortización
│   │
│   ├── ApiGateway/
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   └── SimuladorCreditos.slnx
│
├── database/
│   └── database.sql              Esquema completo, generado de las migraciones
│
├── frontend/
│   └── creditos-web/
│       ├── src/
│       └── package.json
│
├── docs/                         Backlog y evidencias de cada sprint
├── iniciar.bat                   Levanta todo el sistema
├── README.md
└── .gitignore
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

El archivo `backend/AuthService/appsettings.Development.json` está excluido del
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

Crea también `backend/CreditService/appsettings.Development.json`:

```jsonc
{
  "ConnectionStrings": {
    "CreditDb": "Host=localhost;Port=5432;Database=creditdb;Username=postgres;Password=TU_CONTRASEÑA"
  },
  "Jwt": {
    "Issuer": "SimuladorCreditos.AuthApi",
    "Audience": "SimuladorCreditos.Clientes",
    "Key": "LA_MISMA_CLAVE_QUE_EN_AUTHSERVICE"
  },
  "Cors": { "OrigenesPermitidos": [ "http://localhost:5173" ] }
}
```

> **Importante:** `Jwt.Key`, `Issuer` y `Audience` deben ser idénticos en ambos
> servicios. CreditService valida la firma de los tokens que emite AuthService; si
> las claves difieren, rechazará con 401 incluso a usuarios con sesión válida.

No hace falta crear las bases a mano: al arrancar en modo desarrollo, EF Core crea
`authdb` y `creditdb`, aplica las migraciones y siembra el catálogo de tasas.

### 2. Levantar el sistema

La forma corta, desde la raíz del proyecto:

```
iniciar.bat
```

Comprueba los requisitos, instala las dependencias del frontend si faltan, abre
los cuatro componentes en ventanas separadas y lanza el navegador.

Para hacerlo a mano, una terminal por componente:

```bash
cd backend/AuthService
dotnet run --launch-profile http     # http://localhost:5080
```

```bash
cd backend/CreditService
dotnet run --launch-profile http     # http://localhost:5090
```

```bash
cd backend/ApiGateway
dotnet run --launch-profile http     # http://localhost:5000
```

```bash
cd frontend/creditos-web
npm install
npm run dev                          # http://localhost:5173
```

Levanta el gateway **después** de los dos servicios: si recibe una petición antes
de que estén arriba, la reenviará y responderá 502.

Abre `http://localhost:5173`, regístrate y accede al simulador.

### 3. Ejecutar las pruebas

```bash
cd backend
dotnet test SimuladorCreditos.slnx   # 66 pruebas del motor de amortización
```

### 4. Crear el esquema con SQL (opcional)

En desarrollo no hace falta: cada servicio crea su base y aplica sus migraciones
al arrancar. Para entornos donde la aplicación no tiene permisos de DDL:

```bash
psql -U postgres -h localhost -f database/database.sql
```

El script es idempotente y se genera a partir de las migraciones, que siguen
siendo la fuente de la verdad del esquema.
