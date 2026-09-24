@echo off
REM ============================================================================
REM  Sistema de Simulacion de Creditos - arranque completo en desarrollo
REM  Universidad Tecnica de Ambato
REM
REM  Levanta cada componente en su propia ventana para poder leer sus registros
REM  por separado. Cerrar una ventana detiene solo ese componente.
REM
REM    AuthService    http://localhost:5080
REM    CreditService  http://localhost:5090
REM    ApiGateway     http://localhost:5000   <- punto de entrada de la SPA
REM    creditos-web   http://localhost:5173
REM ============================================================================

setlocal
cd /d "%~dp0"

echo.
echo ===========================================================
echo   Sistema de Simulacion de Creditos
echo ===========================================================
echo.

REM --- Comprobaciones previas ------------------------------------------------

where dotnet >nul 2>&1
if errorlevel 1 (
    echo [ERROR] No se encontro el SDK de .NET. Instalalo desde https://dotnet.microsoft.com/download
    goto :fin
)

where npm >nul 2>&1
if errorlevel 1 (
    echo [ERROR] No se encontro Node.js. Instalalo desde https://nodejs.org
    goto :fin
)

REM SQL Server debe estar disponible: sin base, los servicios arrancan y fallan
REM en la primera peticion, que es mucho mas dificil de diagnosticar.
REM
REM Se admiten las dos formas de tenerlo: LocalDB, que arranca bajo demanda y
REM no es un servicio permanente, o una instancia con servicio propio como
REM SQLEXPRESS. Se comprueba el servicio y no un puerto, porque una instancia
REM con nombre no usa necesariamente el 1433.
set "SQL_OK="
set "LOCALDB="

REM El instalador de LocalDB no siempre deja SqlLocalDB.exe en la ruta del
REM sistema, asi que si no se encuentra por nombre se busca donde se instala.
where sqllocaldb >nul 2>&1
if not errorlevel 1 set "LOCALDB=sqllocaldb"
if not defined LOCALDB (
    for /f "delims=" %%E in ('dir /b /s "%ProgramFiles%\Microsoft SQL Server\SqlLocalDB.exe" 2^>nul') do set "LOCALDB=%%E"
)

if defined LOCALDB (
    "%LOCALDB%" start MSSQLLocalDB >nul 2>&1
    if not errorlevel 1 set "SQL_OK=LocalDB"
)

if not defined SQL_OK (
    sc query MSSQL$SQLEXPRESS | findstr /I "RUNNING" >nul 2>&1
    if not errorlevel 1 set "SQL_OK=SQLEXPRESS"
)

if not defined SQL_OK (
    echo [AVISO] No se encontro SQL Server disponible.
    echo         Se busco LocalDB ^(instancia MSSQLLocalDB^) y el servicio MSSQL$SQLEXPRESS.
    echo         Revisa la cadena de conexion en los appsettings.Development.json.
    echo.
    choice /C SN /M "Continuar de todas formas"
    if errorlevel 2 goto :fin
) else (
    echo [0/6] SQL Server disponible mediante %SQL_OK%.
)

REM La configuracion local no se versiona: si falta, los servicios no arrancan.
if not exist "backend\AuthService\appsettings.Development.json" (
    echo [ERROR] Falta backend\AuthService\appsettings.Development.json
    echo         Crealo siguiendo la seccion "Puesta en marcha" del README.
    goto :fin
)
if not exist "backend\CreditService\appsettings.Development.json" (
    echo [ERROR] Falta backend\CreditService\appsettings.Development.json
    echo         Crealo siguiendo la seccion "Puesta en marcha" del README.
    goto :fin
)
if not exist "backend\AssetService\appsettings.Development.json" (
    echo [ERROR] Falta backend\AssetService\appsettings.Development.json
    echo         Crealo siguiendo la seccion "Puesta en marcha" del README.
    goto :fin
)

REM --- Dependencias del frontend ---------------------------------------------

if not exist "frontend\creditos-web\node_modules" (
    echo [1/6] Instalando dependencias del frontend, esto tarda un momento...
    pushd "frontend\creditos-web"
    call npm install
    popd
) else (
    echo [1/6] Dependencias del frontend ya instaladas.
)

REM --- Arranque ---------------------------------------------------------------

echo [2/6] Iniciando AuthService    ^(puerto 5080^)...
start "AuthService  :5080" cmd /k "cd /d ""%~dp0backend\AuthService"" && dotnet run --launch-profile http"

echo [3/6] Iniciando CreditService  ^(puerto 5090^)...
start "CreditService :5090" cmd /k "cd /d ""%~dp0backend\CreditService"" && dotnet run --launch-profile http"

echo [4/6] Iniciando AssetService   ^(puerto 5005^)...
start "AssetService  :5005" cmd /k "cd /d ""%~dp0backend\AssetService"" && dotnet run --launch-profile http"

REM El gateway espera un poco: si arranca antes que los servicios, las primeras
REM peticiones que reenvie fallarian con 502.
echo [5/6] Iniciando ApiGateway     ^(puerto 5000^)...
timeout /t 8 /nobreak >nul
start "ApiGateway   :5000" cmd /k "cd /d ""%~dp0backend\ApiGateway"" && dotnet run --launch-profile http"

echo [6/6] Iniciando creditos-web   ^(puerto 5173^)...
start "creditos-web :5173" cmd /k "cd /d ""%~dp0frontend\creditos-web"" && npm run dev"

echo.
echo Esperando a que el frontend este listo...
timeout /t 12 /nobreak >nul
start "" "http://localhost:5173"

echo.
echo ===========================================================
echo   Todo iniciado. Abre http://localhost:5173
echo   Para detener: cierra las cinco ventanas abiertas.
echo ===========================================================

:fin
echo.
pause
endlocal
