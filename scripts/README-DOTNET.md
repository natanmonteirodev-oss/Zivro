# 🔧 Registro do SDK .NET no Windows

Scripts para registrar, verificar e configurar o SDK .NET no Windows.

## 📋 Arquivos

- **register-dotnet.bat** - Script Batch simples (compatível com CMD)
- **register-dotnet.ps1** - Script PowerShell avançado (recomendado)

## 🚀 Como Usar

### Opção 1: Script PowerShell (Recomendado)

1. Abra o PowerShell como **Administrador**
2. Execute o comando:

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
.\scripts\register-dotnet.ps1
```

Ou execute diretamente:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\register-dotnet.ps1
```

### Opção 2: Script Batch (CMD)

1. Abra o **Prompt de Comando (CMD)** como **Administrador**
2. Navegue até o diretório do projeto:

```cmd
cd C:\Users\natan\repos\Zivro
```

3. Execute o script:

```cmd
.\scripts\register-dotnet.bat
```

## ✅ O Que Esses Scripts Fazem

✅ Verifica a versão do .NET instalada  
✅ Lista todos os SDKs disponíveis  
✅ Lista todos os runtimes disponíveis  
✅ Verifica se .NET está no PATH do sistema  
✅ Exibe informações do ambiente (SO, arquitetura, etc.)  
✅ **PowerShell**: Oferece opção de adicionar ao PATH permanentemente  

## 🔍 Verificação Manual

Se preferir verificar manualmente, use:

```powershell
# Verificar versão
dotnet --version

# Listar SDKs
dotnet --list-sdks

# Listar runtimes
dotnet --list-runtimes

# Localizar dotnet no PATH
where dotnet
```

## 📌 Localizações Comuns do .NET

- **Instalação 64-bit**: `C:\Program Files\dotnet`
- **Instalação 32-bit**: `C:\Program Files (x86)\dotnet`

## ⚠️ Notas Importantes

- ⚠️ Execute como **Administrador** para adicionar ao PATH permanentemente
- ⚠️ Após adicionar ao PATH, **reinicie o PowerShell/CMD** para aplicar as mudanças
- ⚠️ Se o .NET não aparecer no PATH, a instalação pode estar incorreta
- ⚠️ Em caso de problemas, reinstale o .NET SDK do [site oficial](https://dotnet.microsoft.com/download)

## 🔗 Links Úteis

- [Download .NET SDK](https://dotnet.microsoft.com/download)
- [Documentação .NET](https://docs.microsoft.com/dotnet)
- [Variáveis de Ambiente .NET](https://github.com/dotnet/sdk/wiki/Environment-Variables)

---

**Status Atual do Projeto:**  
✅ .NET 10.0.4 instalado  
✅ API rodando em `http://localhost:5000`  
✅ Todos os SDKs registrados corretamente
