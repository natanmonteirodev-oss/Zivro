# Script PowerShell para configurar .NET 8 (nivel Usuario)

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "Configuracao do .NET 8 para o Zivro" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Caminhos
$dotnet8Path = "C:\Program Files (x86)\dotnet"
$dotnet8SdkPath = "$dotnet8Path\sdk\8.0.419"
$currentPath = [System.Environment]::GetEnvironmentVariable("Path", "User")

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
Write-Host "[2/4] Verificando PATH atual do usuario..." -ForegroundColor Yellow
$pathEntries = $currentPath -split ";"
$hasDotnet8User = $false

foreach ($entry in $pathEntries) {
    if ($entry -like "*Program Files (x86)\dotnet*") {
        Write-Host "  32-bit (.NET 8): $entry" -ForegroundColor Cyan
        $hasDotnet8User = $true
    }
}

# 3. Adicionar .NET 8 ao PATH do usuario
Write-Host ""
Write-Host "[3/4] Adicionando .NET 8 ao PATH do usuario..." -ForegroundColor Yellow

if (-not $hasDotnet8User) {
    $newPath = "C:\Program Files (x86)\dotnet"
    if ($currentPath) {
        $newPath = "$newPath;$currentPath"
    }
    [System.Environment]::SetEnvironmentVariable("Path", $newPath, "User")
    Write-Host "[OK] .NET 8 adicionado ao PATH do usuario!" -ForegroundColor Green
} else {
    Write-Host "[INFO] .NET 8 ja esta no PATH" -ForegroundColor Cyan
}

# 4. Configurar variavel DOTNET_ROOT(x86)
Write-Host ""
Write-Host "[4/4] Configurando variaveis de ambiente..." -ForegroundColor Yellow
[System.Environment]::SetEnvironmentVariable("DOTNET_ROOT(x86)", "C:\Program Files (x86)\dotnet", "User")
Write-Host "[OK] DOTNET_ROOT(x86) configurada" -ForegroundColor Green

# Verificar global.json
Write-Host ""
$globalJsonPath = "C:\Users\natan\repos\Zivro\backend\global.json"
if (Test-Path $globalJsonPath) {
    Write-Host "[OK] global.json existe no backend" -ForegroundColor Green
    $content = Get-Content $globalJsonPath | ConvertFrom-Json
    Write-Host "     Versao do SDK: $($content.sdk.version)" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "Configuracao Concluida com Sucesso!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "PROXIMAS ACOES:" -ForegroundColor Yellow
Write-Host "1. Feche TODAS as abas do PowerShell/CMD" -ForegroundColor Cyan
Write-Host "2. Abra uma nova janela do PowerShell/CMD" -ForegroundColor Cyan
Write-Host "3. Execute: dotnet --version" -ForegroundColor Cyan
Write-Host "4. Execute: dotnet --list-sdks" -ForegroundColor Cyan
Write-Host "5. Navegue para backend, execute: dotnet build" -ForegroundColor Cyan
Write-Host ""

Read-Host "Pressione ENTER para sair"
