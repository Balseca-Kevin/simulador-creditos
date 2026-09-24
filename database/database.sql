-- =============================================================================
--  Sistema de Simulación de Créditos — esquema completo de base de datos
--  Universidad Técnica de Ambato
--  Motor: Microsoft SQL Server
-- =============================================================================
--
--  Cada microservicio es dueño de su propia base (patrón Database per Service):
--    · authdb   -> AuthService   : identidades de los usuarios
--    · creditdb -> CreditService : catálogo de productos e historial de simulaciones
--    · assetdb  -> AssetService  : garantías declaradas y sus categorías
--
--  Ejecución:
--      sqlcmd -S <instancia> -E -i database\database.sql
--  o abriendo el archivo en SQL Server Management Studio y pulsando Ejecutar.
--
--  Este archivo NO es la fuente de la verdad: se genera a partir de las
--  migraciones de EF Core, que sí lo son. Para regenerarlo tras un cambio de
--  modelo, en cada servicio:
--      dotnet ef migrations script --idempotent -o <servicio>.sql
--  y volver a unirlos bajo estas cabeceras, quitando la marca BOM que EF Core
--  escribe al inicio de cada archivo: incrustada a mitad del script, el
--  intérprete la leería como parte de una sentencia y fallaría.
--
--  Los scripts son idempotentes: se pueden ejecutar varias veces sin duplicar
--  objetos ni datos.
--
--  Nota: en desarrollo no hace falta ejecutar nada de esto. Al arrancar, cada
--  servicio crea su base y aplica sus migraciones automáticamente. Este archivo
--  existe para despliegues donde la aplicación no tiene permiso de DDL, y como
--  documentación legible del esquema.
-- =============================================================================

-- Detiene la ejecución ante el primer error. Sin esto, si una base no se
-- pudiera abrir, el script seguiría adelante y crearía sus tablas en la base
-- que estuviera activa en ese momento: fallaría en silencio y en el lugar
-- equivocado. Funciona con sqlcmd y con el modo SQLCMD de SQL Server
-- Management Studio; si se ejecuta sin él, la comprobación de DB_NAME() que
-- hay tras cada USE sigue sirviendo de red de seguridad.
:on error exit


-- =============================================================================
--  1. authdb
-- =============================================================================

IF DB_ID('authdb') IS NULL
    CREATE DATABASE [authdb];
GO

USE [authdb];
GO

IF DB_NAME() <> 'authdb'
    THROW 50000, 'No se pudo cambiar a la base authdb. Se detiene el script.', 1;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114401_CreacionInicial'
)
BEGIN
    CREATE TABLE [usuarios] (
        [Id] uniqueidentifier NOT NULL,
        [NombreCompleto] nvarchar(150) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [PasswordHash] nvarchar(255) NOT NULL,
        [FechaRegistro] datetime2 NOT NULL,
        CONSTRAINT [PK_usuarios] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114401_CreacionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_usuarios_Email] ON [usuarios] ([Email]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114401_CreacionInicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924114401_CreacionInicial', N'10.0.12');
END;

COMMIT;
GO


-- =============================================================================
--  2. creditdb
-- =============================================================================

IF DB_ID('creditdb') IS NULL
    CREATE DATABASE [creditdb];
GO

USE [creditdb];
GO

IF DB_NAME() <> 'creditdb'
    THROW 50000, 'No se pudo cambiar a la base creditdb. Se detiene el script.', 1;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114413_CreacionInicial'
)
BEGIN
    CREATE TABLE [tipos_credito] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(30) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Categoria] nvarchar(40) NOT NULL,
        [TasaAnual] decimal(5,2) NOT NULL,
        [TasaSeguroDesgravamenMensual] decimal(7,4) NOT NULL,
        [Descripcion] nvarchar(300) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_tipos_credito] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114413_CreacionInicial'
)
BEGIN
    CREATE TABLE [simulaciones] (
        [Id] uniqueidentifier NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [TipoCreditoId] int NOT NULL,
        [Monto] decimal(18,2) NOT NULL,
        [PlazoMeses] int NOT NULL,
        [FrecuenciaPago] nvarchar(20) NOT NULL,
        [IncluyeSeguroDesgravamen] bit NOT NULL,
        [TasaAnualAplicada] decimal(5,2) NOT NULL,
        [CuotaFija] decimal(18,2) NOT NULL,
        [TotalInteresFrances] decimal(18,2) NOT NULL,
        [TotalInteresAleman] decimal(18,2) NOT NULL,
        [IngresoMinimoRequerido] decimal(18,2) NOT NULL,
        [FechaSimulacion] datetime2 NOT NULL,
        CONSTRAINT [PK_simulaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_simulaciones_tipos_credito_TipoCreditoId] FOREIGN KEY ([TipoCreditoId]) REFERENCES [tipos_credito] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114413_CreacionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Categoria', N'Codigo', N'Descripcion', N'Nombre', N'TasaAnual', N'TasaSeguroDesgravamenMensual') AND [object_id] = OBJECT_ID(N'[tipos_credito]'))
        SET IDENTITY_INSERT [tipos_credito] ON;
    EXEC(N'INSERT INTO [tipos_credito] ([Id], [Activo], [Categoria], [Codigo], [Descripcion], [Nombre], [TasaAnual], [TasaSeguroDesgravamenMensual])
    VALUES (1, CAST(1 AS bit), N''Consumo'', N''CONSUMO'', N''Adquisición de bienes de consumo o pago de servicios.'', N''Crédito de Consumo'', 15.5, 0.05),
    (2, CAST(1 AS bit), N''Vivienda'', N''INMOBILIARIO'', N''Compra, construcción o remodelación de vivienda.'', N''Crédito Inmobiliario'', 8.5, 0.04),
    (3, CAST(1 AS bit), N''Microcrédito'', N''MICROCREDITO'', N''Financiamiento para actividades productivas a pequeña escala.'', N''Microcrédito'', 22.0, 0.07),
    (4, CAST(1 AS bit), N''Productivo'', N''PRODUCTIVO_CORPORATIVO'', N''Empresas con ventas anuales superiores a cinco millones de dólares.'', N''Productivo Corporativo'', 6.79, 0.03),
    (5, CAST(1 AS bit), N''Productivo'', N''PRODUCTIVO_EMPRESARIAL'', N''Empresas con ventas anuales entre uno y cinco millones de dólares.'', N''Productivo Empresarial'', 8.62, 0.035),
    (6, CAST(1 AS bit), N''Productivo'', N''PRODUCTIVO_PYMES'', N''Pequeñas y medianas empresas con ventas anuales de hasta un millón de dólares.'', N''Productivo PYMES'', 9.18, 0.045),
    (7, CAST(1 AS bit), N''Educativo'', N''EDUCATIVO'', N''Financiamiento de estudios de grado, posgrado y formación profesional.'', N''Crédito Educativo'', 8.95, 0.045),
    (8, CAST(1 AS bit), N''Educativo'', N''EDUCATIVO_SOCIAL'', N''Estudios para personas en situación de vulnerabilidad, con tasa preferente.'', N''Crédito Educativo Social'', 5.49, 0.04),
    (9, CAST(1 AS bit), N''Vivienda'', N''VIVIENDA_INTERES_SOCIAL'', N''Primera vivienda para familias de bajos ingresos, con tope de precio regulado.'', N''Vivienda de Interés Social'', 4.99, 0.04),
    (10, CAST(1 AS bit), N''Vivienda'', N''VIVIENDA_INTERES_PUBLICO'', N''Primera vivienda dentro de proyectos calificados por el Estado.'', N''Vivienda de Interés Público'', 4.99, 0.04)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Categoria', N'Codigo', N'Descripcion', N'Nombre', N'TasaAnual', N'TasaSeguroDesgravamenMensual') AND [object_id] = OBJECT_ID(N'[tipos_credito]'))
        SET IDENTITY_INSERT [tipos_credito] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114413_CreacionInicial'
)
BEGIN
    CREATE INDEX [IX_simulaciones_TipoCreditoId] ON [simulaciones] ([TipoCreditoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114413_CreacionInicial'
)
BEGIN
    CREATE INDEX [IX_simulaciones_UsuarioId_FechaSimulacion] ON [simulaciones] ([UsuarioId], [FechaSimulacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114413_CreacionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_tipos_credito_Codigo] ON [tipos_credito] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114413_CreacionInicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924114413_CreacionInicial', N'10.0.12');
END;

COMMIT;
GO


-- =============================================================================
--  3. assetdb
-- =============================================================================

IF DB_ID('assetdb') IS NULL
    CREATE DATABASE [assetdb];
GO

USE [assetdb];
GO

IF DB_NAME() <> 'assetdb'
    THROW 50000, 'No se pudo cambiar a la base assetdb. Se detiene el script.', 1;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114424_CreacionInicial'
)
BEGIN
    CREATE TABLE [categorias_activo] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(30) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(300) NOT NULL,
        [Habilitada] bit NOT NULL,
        CONSTRAINT [PK_categorias_activo] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114424_CreacionInicial'
)
BEGIN
    CREATE TABLE [activos] (
        [Id] uniqueidentifier NOT NULL,
        [UsuarioId] uniqueidentifier NOT NULL,
        [CategoriaId] int NOT NULL,
        [Nombre] nvarchar(150) NOT NULL,
        [Descripcion] nvarchar(300) NOT NULL,
        [ValorEstimado] decimal(18,2) NOT NULL,
        [FechaAdquisicion] date NOT NULL,
        [FechaRegistro] datetime2 NOT NULL,
        [FechaActualizacion] datetime2 NULL,
        CONSTRAINT [PK_activos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_activos_categorias_activo_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [categorias_activo] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114424_CreacionInicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Codigo', N'Descripcion', N'Habilitada', N'Nombre') AND [object_id] = OBJECT_ID(N'[categorias_activo]'))
        SET IDENTITY_INSERT [categorias_activo] ON;
    EXEC(N'INSERT INTO [categorias_activo] ([Id], [Codigo], [Descripcion], [Habilitada], [Nombre])
    VALUES (1, N''VEHICULO'', N''Automóviles, motocicletas y vehículos de carga.'', CAST(1 AS bit), N''Vehículo''),
    (2, N''INMUEBLE'', N''Casas, departamentos, terrenos y locales comerciales.'', CAST(1 AS bit), N''Inmueble''),
    (3, N''MAQUINARIA'', N''Equipos productivos, herramientas y maquinaria industrial.'', CAST(1 AS bit), N''Maquinaria y equipo''),
    (4, N''INVERSION'', N''Depósitos a plazo, acciones y participaciones.'', CAST(1 AS bit), N''Inversión financiera''),
    (5, N''OTRO'', N''Bienes que no encajan en las categorías anteriores.'', CAST(1 AS bit), N''Otros bienes'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Codigo', N'Descripcion', N'Habilitada', N'Nombre') AND [object_id] = OBJECT_ID(N'[categorias_activo]'))
        SET IDENTITY_INSERT [categorias_activo] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114424_CreacionInicial'
)
BEGIN
    CREATE INDEX [IX_activos_CategoriaId] ON [activos] ([CategoriaId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114424_CreacionInicial'
)
BEGIN
    CREATE INDEX [IX_activos_UsuarioId_FechaRegistro] ON [activos] ([UsuarioId], [FechaRegistro]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114424_CreacionInicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_categorias_activo_Codigo] ON [categorias_activo] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924114424_CreacionInicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924114424_CreacionInicial', N'10.0.12');
END;

COMMIT;
GO

