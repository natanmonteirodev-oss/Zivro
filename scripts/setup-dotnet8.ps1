# Script PowerShell para configurar .NET 8 no PATH

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Configuracao do .NET 8 para o Zivro" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Caminhos
$dotnet8Path = "C:\Program Files (x86)\dotnet"
$dotnet8SdkPath = "$dotnet8Path\sdk\8.0.419"
$currentPath = [System.Environment]::GetEnvironmentVariable("Path", "Machine")

# 1. Verificar se .NET 8 existe
Write-Host "[1/4] Verificando se .NET 8 existe..." -ForegroundColor Yellow
if (Test-Path $dotnet8SdkPath) {
    Write-Host "[OK] .NET 8.0.419 encontrado em: $dotnet8SdkPath" -ForegroundColor Green
} else {
    Write-Host "[ERRO] .NET 8.0.419 nao encontrado!" -ForegroundColor Red
    exit 1
}

# 2. Verificar se estao no PATH
Write-Host ""
Write-Host "[2/4] Verificando PATH atual..." -ForegroundColor Yellow
$pathEntries = $currentPath -split ";"
$hasDotnet10 = $false
$hasDotnet8 = $false

foreach ($entry in $pathEntries) {
    if ($entry -like "*Program Files\dotnet*") {
        Write-Host "  64-bit (.NET 10): $entry" -ForegroundColor Cyan
        $hasDotnet10 = $true
    }
    if ($entry -like "*Program Files (x86)\dotnet*") {
        Write-Host "  32-bit (.NET 8): $entry" -ForegroundColor Cyan
        $hasDotnet8 = $true
    }
}

# 3. Reordenar PATH para priorizar .NET 8
Write-Host ""
Write-Host "[3/4] Atualizando PATH para priorizar .NET 8..." -ForegroundColor Yellow

$newPath = $currentPath
# Remove todas as entradas de dotnet
$newPath = $newPath -replace "C:\\Program Files\\dotnet\\?;?", ""
$newPath = $newPath -replace "C:\\Program Files \(x86\)\\dotnet\\?;?", ""

# Adiciona .NET 8 em primeiro lugar
$newPath = "C:\Program Files (x86)\dotnet;$newPath"

# Remove paths vazios duplos
$newPath = $newPath -replace ";+", ";"
$newPath = $newPath -replace "^;|;$", ""

[System.Environment]::SetEnvironmentVariable("Path", $newPath, "Machine")
Write-Host "[OK] PATH atualizado com sucesso!" -ForegroundColor Green

# 4. Verificar e informar sobre global.json
Write-Host ""
Write-Host "[4/4] Verificando configuracao do SDK..." -ForegroundColor Yellow
$globalJsonPath = "C:\Users\natan\repos\Zivro\backend\global.json"
if (Test-Path $globalJsonPath) {
    Write-Host "[OK] global.json existe no backend" -ForegroundColor Green
    $content = Get-Content $globalJsonPath | ConvertFrom-Json
    Write-Host "     Versao do SDK: $($content.sdk.version)" -ForegroundColor Cyan
} else {
    Write-Host "[AVISO] global.json nao encontrado" -ForegroundColor Yellow
}

# Informar sobre variavel de ambiente
Write-Host ""
Write-Host "[INFO] Variaveis de ambiente relacionadas:" -ForegroundColor Yellow
$dotnetRoot = [System.Environment]::GetEnvironmentVariable("DOTNET_ROOT", "Machine")
$dotnetRootX86 = [System.Environment]::GetEnvironmentVariable("DOTNET_ROOT(x86)", "Machine")

if ($dotnetRoot) {
    Write-Host "  DOTNET_ROOT: $dotnetRoot" -ForegroundColor Cyan
} else {
    Write-Host "  DOTNET_ROOT: nao definido" -ForegroundColor Yellow
}

if ($dotnetRootX86) {
    Write-Host "  DOTNET_ROOT(x86): $dotnetRootX86" -ForegroundColor Cyan
} else {
    Write-Host "  DOTNET_ROOT(x86): nao definido" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Configuracao Concluida!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "PROXIMAS ACOES:" -ForegroundColor Yellow
Write-Host "1. Feche e reabra o PowerShell/CMD para aplicar as mudancas" -ForegroundColor Cyan
Write-Host "2. Execute: dotnet --version" -ForegroundColor Cyan
Write-Host "3. Execute: dotnet --list-sdks" -ForegroundColor Cyan
Write-Host "4. No diretorio do backend, execute: dotnet build" -ForegroundColor Cyan
Write-Host ""

Read-Host "Pressione ENTER para sair"
