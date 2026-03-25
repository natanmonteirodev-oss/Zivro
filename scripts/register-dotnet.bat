@echo off
REM Script para registrar/verificar o SDK do .NET no Windows

echo.
echo ============================================
echo Verificando e Registrando SDK .NET
echo ============================================
echo.

REM Verificar versão do .NET
echo [INFO] Verificando versao instalada do .NET...
dotnet --version
if errorlevel 1 (
    echo [ERRO] .NET nao foi encontrado no PATH
    echo [INFO] Adicione manualmente: C:\Program Files\dotnet ao PATH do Windows
    pause
    exit /b 1
)

REM Listar todos os SDKs instalados
echo.
echo [INFO] SDKs .NET instalados:
dotnet --list-sdks
echo.

REM Listar todos os runtimes instalados
echo [INFO] Runtimes .NET instalados:
dotnet --list-runtimes
echo.

REM Verificar PATH
echo [INFO] Verificando PATH do sistema...
where dotnet >nul 2>&1
if %errorlevel% equ 0 (
    echo [SUCESSO] .NET esta registrado no PATH
    where dotnet
) else (
    echo [AVISO] .NET nao esta no PATH do sistema
    echo [INFO] Localizacao do .NET:
    for /d %%D in ("C:\Program Files\dotnet") do (
        if exist "%%D\dotnet.exe" (
            echo [ENCONTRADO] %%D\dotnet.exe
        )
    )
)

echo.
echo [INFO] Registro do .NET completado com sucesso!
pause
