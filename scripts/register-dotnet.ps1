# Script PowerShell para registrar/verificar o SDK do .NET no Windows

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Verificando e Registrando SDK .NET" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# 1. Verificar versão do .NET
Write-Host "[INFO] Verificando versao instalada do .NET..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "[SUCESSO] .NET versao: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "[ERRO] .NET nao foi encontrado!" -ForegroundColor Red
    exit 1
}

# 2. Listar SDKs instalados
Write-Host ""
Write-Host "[INFO] SDKs .NET instalados:" -ForegroundColor Yellow
dotnet --list-sdks

# 3. Listar runtimes instalados
Write-Host ""
Write-Host "[INFO] Runtimes .NET instalados:" -ForegroundColor Yellow
dotnet --list-runtimes

# 4. Verificar localizacao no PATH
Write-Host ""
Write-Host "[INFO] Verificando PATH do sistema..." -ForegroundColor Yellow

$dotnetPath = (Get-Command dotnet -ErrorAction SilentlyContinue).Source
if ($dotnetPath) {
    Write-Host "[SUCESSO] .NET esta no PATH" -ForegroundColor Green
    Write-Host "Localizacao: $dotnetPath" -ForegroundColor Green
} else {
    Write-Host "[AVISO] .NET nao foi encontrado no PATH" -ForegroundColor Yellow
    
    # Tentar localizar manualmente
    $possiblePaths = @(
        "C:\Program Files\dotnet\dotnet.exe",
        "C:\Program Files (x86)\dotnet\dotnet.exe"
    )
    
    foreach ($path in $possiblePaths) {
        if (Test-Path $path) {
            Write-Host "Encontrado em: $path" -ForegroundColor Green
        }
    }
}

# 5. Informacoes adicionais do ambiente
Write-Host ""
Write-Host "[INFO] Informacoes do ambiente:" -ForegroundColor Yellow
Write-Host "SO: $([System.Environment]::OSVersion.VersionString)" -ForegroundColor Cyan
Write-Host "Arquitetura: $(& {if ([System.Environment]::Is64BitProcess) { '64-bit' } else { '32-bit' }})" -ForegroundColor Cyan
Write-Host "PowerShell: $($PSVersionTable.PSVersion.ToString())" -ForegroundColor Cyan

# 6. Diretorio de instalacao
$dotnetRoot = [System.Environment]::GetEnvironmentVariable("DOTNET_ROOT", "Machine")
if ($dotnetRoot) {
    Write-Host "DOTNET_ROOT: $dotnetRoot" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "[OK] Registro do .NET completado!" -ForegroundColor Green
Write-Host ""

# Opcao de adicionar ao PATH permanentemente
Write-Host "Deseja adicionar .NET ao PATH permanentemente? (S/N)" -ForegroundColor Yellow
$response = Read-Host

if ($response -eq "S" -or $response -eq "s") {
    $dotnetInstallPath = "C:\Program Files\dotnet"
    
    if (Test-Path $dotnetInstallPath) {
        $currentPath = [System.Environment]::GetEnvironmentVariable("Path", "Machine")
        
        if ($currentPath -notlike "*dotnet*") {
            $newPath = "$currentPath;$dotnetInstallPath"
            [System.Environment]::SetEnvironmentVariable("Path", $newPath, "Machine")
            Write-Host "[SUCESSO] .NET adicionado ao PATH permanentemente!" -ForegroundColor Green
            Write-Host "Reinicie o PowerShell para aplicar as mudancas." -ForegroundColor Yellow
        } else {
            Write-Host "[INFO] .NET ja esta no PATH" -ForegroundColor Green
        }
    } else {
        Write-Host "[ERRO] Diretorio de instalacao nao encontrado!" -ForegroundColor Red
    }
}
