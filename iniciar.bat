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

REM PostgreSQL debe estar escuchando: sin base, los servicios arrancan y fallan
REM en la primera peticion, que es mucho mas dificil de diagnosticar.
netstat -an | findstr /C:"127.0.0.1:5432" | findstr /I "LISTENING" >nul 2>&1
if errorlevel 1 (
    netstat -an | findstr /C:"0.0.0.0:5432" | findstr /I "LISTENING" >nul 2>&1
    if errorlevel 1 (
        echo [AVISO] No hay nada escuchando en el puerto 5432.
        echo         Revisa que el servicio de PostgreSQL este iniciado.
        echo.
        choice /C SN /M "Continuar de todas formas"
        if errorlevel 2 goto :fin
    )
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

REM --- Dependencias del frontend ---------------------------------------------

if not exist "frontend\creditos-web\node_modules" (
    echo [1/5] Instalando dependencias del frontend, esto tarda un momento...
    pushd "frontend\creditos-web"
    call npm install
    popd
) else (
    echo [1/5] Dependencias del frontend ya instaladas.
)

REM --- Arranque ---------------------------------------------------------------

echo [2/5] Iniciando AuthService    ^(puerto 5080^)...
start "AuthService  :5080" cmd /k "cd /d ""%~dp0backend\AuthService"" && dotnet run --launch-profile http"

echo [3/5] Iniciando CreditService  ^(puerto 5090^)...
start "CreditService :5090" cmd /k "cd /d ""%~dp0backend\CreditService"" && dotnet run --launch-profile http"

REM El gateway espera un poco: si arranca antes que los servicios, las primeras
REM peticiones que reenvie fallarian con 502.
echo [4/5] Iniciando ApiGateway     ^(puerto 5000^)...
timeout /t 8 /nobreak >nul
start "ApiGateway   :5000" cmd /k "cd /d ""%~dp0backend\ApiGateway"" && dotnet run --launch-profile http"

echo [5/5] Iniciando creditos-web   ^(puerto 5173^)...
start "creditos-web :5173" cmd /k "cd /d ""%~dp0frontend\creditos-web"" && npm run dev"

echo.
echo Esperando a que el frontend este listo...
timeout /t 12 /nobreak >nul
start "" "http://localhost:5173"

echo.
echo ===========================================================
echo   Todo iniciado. Abre http://localhost:5173
echo   Para detener: cierra las cuatro ventanas abiertas.
echo ===========================================================

:fin
echo.
pause
endlocal
