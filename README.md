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
| `creditos-web` | 5173 | React 19 + Vite + TypeScript + Tailwind CSS v4 | SPA: login, simulación y garantías |
| `ApiGateway` | 5000 | .NET 10 + YARP | Punto de entrada único; enruta hacia cada servicio |
| `AuthService` | 5080 | .NET 10 (ASP.NET Core) | Registro, login y emisión de tokens JWT |
| `CreditService` | 5090 | .NET 10 (ASP.NET Core) | Motor de cálculo, reglas de amortización y reporte PDF |
| `AssetService` | 5005 | .NET 10 (ASP.NET Core) | Activos y categorías: garantías declaradas por el usuario |
| Persistencia | 1433 | SQL Server Express + EF Core | `authdb`, `creditdb` y `assetdb`, una por servicio |

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
                      └──┬────────┬────┬─┘
               /api/auth │        │    │ /api/activos
                         │        │    │ /api/categorias
                         │        │    └──────────────┐
                         │        │ /api/creditos     │
                         ▼        ▼                   ▼
            ┌───────────────┐ ┌───────────────┐ ┌───────────────┐
            │  AuthService  │ │ CreditService │ │ AssetService  │
            │     :5080     │ │     :5090     │ │     :5005     │
            └───────┬───────┘ └───────┬───────┘ └───────┬───────┘
                    ▼                 ▼                 ▼
               [ authdb ]        [ creditdb ]      [ assetdb ]
                    SQL Server Express  SQLEXPRESS
```

Los tres servicios son independientes: **ninguno llama a otro**. El patrimonio
declarado aparece junto al ingreso mínimo porque la SPA consulta a los dos y une
los resultados, no porque CreditService conozca a AssetService.

### Arquitectura Onion dentro de cada microservicio

Cada capa es **un proyecto propio**, no una carpeta. Las referencias apuntan solo
hacia adentro, así que romper una capa deja de ser un descuido posible y pasa a
ser un error de compilación.

| Proyecto | Contiene | Referencia a |
|---|---|---|
| `Dominio` | Entidades y reglas propias del negocio | **nada** |
| `Aplicacion` | Contratos, DTOs y casos de uso | `Dominio` |
| `Estructura` | EF Core, repositorios, generación de PDF | `Aplicacion` |
| `Presentacion` | Controladores y formato de las respuestas HTTP | `Aplicacion` |
| *(anfitrión)* | `Program.cs`, configuración y `Migrations/` | `Estructura`, `Presentacion` |

La dependencia con la base de datos está **invertida**: `Aplicacion` declara qué
necesita (`IRepositorioUsuarios`, `IRepositorioCreditos`, `IRepositorioActivos`)
y `Estructura` lo implementa. Las dos se encuentran solo en el anfitrión, al
arrancar.

Como consecuencia, `Aplicacion` no conoce EF Core y `Presentacion` no conoce la
base de datos: los controladores solo traducen entre HTTP y casos de uso, y el
`Resultado<T>` que reciben indica el motivo del fallo sin hablar de códigos HTTP.

Comprobado añadiendo a propósito un `using CreditService.Estructura` dentro de
`Aplicacion`: el compilador lo rechaza con `error CS0234`.

## Reglas de negocio

El tipo de crédito determina la tasa de interés anual y la prima del seguro de
desgravamen aplicadas:

| Categoría | Tipo de crédito | Tasa anual | Desgravamen mensual |
|---|---|---|---|
| Consumo | Crédito de Consumo | 15.50 % | 0.0500 % |
| Vivienda | Crédito Inmobiliario | 8.50 % | 0.0400 % |
| Vivienda | Vivienda de Interés Social | 4.99 % | 0.0400 % |
| Vivienda | Vivienda de Interés Público | 4.99 % | 0.0400 % |
| Microcrédito | Microcrédito | 22.00 % | 0.0700 % |
| Productivo | Productivo Corporativo | 6.79 % | 0.0300 % |
| Productivo | Productivo Empresarial | 8.62 % | 0.0350 % |
| Productivo | Productivo PYMES | 9.18 % | 0.0450 % |
| Educativo | Crédito Educativo | 8.95 % | 0.0450 % |
| Educativo | Crédito Educativo Social | 5.49 % | 0.0400 % |

Las tres primeras filas conservan la tasa fijada en la sección 3 del documento
oficial del proyecto. El resto corresponde a los segmentos del Banco Central del
Ecuador, con sus **tasas activas efectivas referenciales de agosto de 2026**.

Las tasas se almacenan en `creditdb`, no están fijadas en el código: la regla es
dinámica y agregar un tipo nuevo es una migración con una fila más, sin tocar el
motor ni la interfaz. Las primas de desgravamen son valores referenciales de
mercado: más bajas a mayor plazo y garantía, más altas a mayor riesgo.

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

### Reporte en PDF

Cada simulación puede abrirse como un reporte de varias páginas: portada con
los datos del crédito y la comparación de los dos métodos, y una tabla completa
por método. Se genera en la Credit API a partir de la misma respuesta que ve la
pantalla, de modo que ambas no pueden discrepar, y se abre en el visor del
navegador, que aporta paginación, impresión y descarga.

Para abrir la pestaña se emite un enlace de un solo uso con dos minutos de
vigencia: el navegador no puede enviar la cabecera de autorización al navegar, y
poner el token de sesión en la URL lo dejaría en el historial y en los registros.

### Ingreso mínimo requerido

La cuota más alta de la tabla, llevada a su equivalente mensual, dividida para
0.40: la cuota no debe superar el **40 % del ingreso**. Se redondea hacia arriba
al centavo para no quedar nunca por debajo del umbral.

### Garantías declaradas

El usuario registra los bienes que respaldan su solicitud —vehículos, inmuebles,
maquinaria, inversiones u otros— con su valor estimado y su fecha de adquisición.
Las cinco categorías viven en `assetdb` y se administran por migración, igual que
las tasas.

Cada bien queda ligado al usuario del token, y las consultas filtran por él: pedir
un activo ajeno por su identificador devuelve 404, no el bien de otra persona.

El simulador muestra el patrimonio total junto al ingreso mínimo requerido, para
poder mirar la cuota y el respaldo a la vez.

## Estructura del repositorio

```
proyecto_SimuladorDeCreditos/
│
├── backend/
│   │
│   ├── AuthService/              Cada capa es un proyecto independiente
│   │   ├── Dominio/                AuthService.Dominio.csproj
│   │   ├── Aplicacion/             AuthService.Aplicacion.csproj
│   │   ├── Estructura/             AuthService.Estructura.csproj
│   │   ├── Presentacion/           AuthService.Presentacion.csproj
│   │   ├── Migrations/
│   │   └── Program.cs              AuthService.csproj (anfitrión)
│   │
│   ├── CreditService/            Misma estructura de capas
│   │   ├── Dominio/
│   │   ├── Aplicacion/
│   │   ├── Estructura/
│   │   ├── Presentacion/
│   │   ├── Migrations/
│   │   └── Program.cs
│   │
│   ├── AssetService/             Misma estructura de capas
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
│   └── database.sql              Las 3 bases, generado de las migraciones
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

### Guía para un nuevo colaborador

Después de clonar el repositorio, el proyecto se puede levantar desde una instalación limpia.
No es necesario copiar `bin/`, `obj/`, `node_modules/` ni `dist/`: se regeneran localmente.

```bash
git clone https://github.com/Balseca-Kevin/simulador-creditos.git
cd simulador-creditos
```

### Requisitos

- Git
- .NET SDK 10
- Node.js 22 o superior
- SQL Server: sirve **LocalDB**, que es lo que usa el equipo, o una instancia
  con servicio propio como **Express**

Comprueba las versiones con `git --version`, `dotnet --version`, `node --version`
y `npm --version`. Para SQL Server, `sqllocaldb info` debe listar `MSSQLLocalDB`.

> **Sobre LocalDB.** Es el motor de SQL Server en su forma más ligera: arranca
> bajo demanda, no ocupa un servicio permanente y solo acepta conexiones locales.
> Para desarrollo y para la demostración se comporta igual que Express: mismo
> T-SQL, mismas migraciones, mismo proveedor de EF Core. Cambiar a Express o a un
> servidor remoto es editar la cadena de conexión, nada más.

Se usa **autenticación de Windows**, así que no hay contraseñas de base de datos
en ningún archivo. Tu usuario necesita permiso para crear bases, que en LocalDB
tiene por omisión al ser el propietario de la instancia.

### 1. Configurar las credenciales locales

El archivo `backend/AuthService/appsettings.Development.json` está excluido del
control de versiones porque contiene la clave de firma de los tokens. Créalo a
partir de esta plantilla:

```jsonc
{
  "ConnectionStrings": {
    "AuthDb": "Server=(localdb)\\MSSQLLocalDB;Database=authdb;Trusted_Connection=True;TrustServerCertificate=True"
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
    "CreditDb": "Server=(localdb)\\MSSQLLocalDB;Database=creditdb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "SimuladorCreditos.AuthApi",
    "Audience": "SimuladorCreditos.Clientes",
    "Key": "LA_MISMA_CLAVE_QUE_EN_AUTHSERVICE"
  },
  "Cors": { "OrigenesPermitidos": [ "http://localhost:5173" ] }
}
```

Y `backend/AssetService/appsettings.Development.json`:

```jsonc
{
  "ConnectionStrings": {
    "AssetDb": "Server=(localdb)\\MSSQLLocalDB;Database=assetdb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Issuer": "SimuladorCreditos.AuthApi",
    "Audience": "SimuladorCreditos.Clientes",
    "Key": "LA_MISMA_CLAVE_QUE_EN_AUTHSERVICE"
  },
  "Cors": { "OrigenesPermitidos": [ "http://localhost:5173" ] }
}
```

> **Importante:** `Jwt.Key`, `Issuer` y `Audience` deben ser idénticos en los tres
> servicios. CreditService y AssetService validan la firma de los tokens que emite
> AuthService; si las claves difieren, rechazarán con 401 incluso a usuarios con
> sesión válida.

Estos tres archivos están excluidos por `.gitignore`, así que cada persona debe
crearlos en su propia copia. No se debe publicar una contraseña ni una clave JWT.

No hace falta arrancar nada a mano: `iniciar.bat` levanta LocalDB si hace falta.
Tampoco hay que crear las bases: en desarrollo EF Core crea `authdb`, `creditdb`
y `assetdb`, aplica las migraciones y siembra los catálogos. Si el usuario no
puede crear bases, un administrador debe crearlas previamente o concederle ese
permiso.

### 2. Levantar el sistema

La forma corta, desde la raíz del proyecto en Windows, es:

```
iniciar.bat
```

Comprueba los requisitos, instala las dependencias del frontend si faltan, abre
los cuatro componentes en ventanas separadas y lanza el navegador.

En PowerShell, si `npm` está bloqueado por la política de ejecución, usa `npm.cmd`:

```powershell
cd frontend/creditos-web
npm.cmd install
npm.cmd run build
```

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
cd backend/AssetService
dotnet run --launch-profile http     # http://localhost:5005
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

Para comprobar que todo quedó disponible, visita `/health` en los puertos `5080`,
`5090` y `5000`, y abre `http://localhost:5173`. Los tres endpoints de backend
deben responder con HTTP `200`. Si aparece `address already in use`, ya existe
otro proceso usando ese puerto; ciérralo o reutiliza el proceso existente.

### 3. Ejecutar las pruebas

```bash
cd backend
dotnet test SimuladorCreditos.slnx   # 66 pruebas del motor de amortización
```

### 4. Crear el esquema con SQL (opcional)

En desarrollo no hace falta: cada servicio crea su base y aplica sus migraciones
al arrancar. Para entornos donde la aplicación no tiene permisos de DDL:

```bash
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i database\database.sql
```

También se puede abrir en SQL Server Management Studio, activando el **modo
SQLCMD** para que se respete la directiva que detiene el script ante el primer
error.

El script es idempotente —ejecutarlo dos veces no duplica nada— y se genera a
partir de las migraciones, que siguen siendo la fuente de la verdad del esquema.
