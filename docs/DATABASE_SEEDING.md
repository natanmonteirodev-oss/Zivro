# Database Seeding Guide

## Overview

A base de dados é populada automaticamente com dados iniciais quando a aplicação inicia. Atualmente, um usuário power user é criado automaticamente.

---

## Dados Iniciais (Seed Data)

### Power User (Usuário Administrativo)

Criado automaticamente na primeira execução:

```
Email: poweruser@zivro.com
Senha: PowerUser@123456
ID: 00000000-0000-0000-0000-000000000001
Status: Ativo
```

**Nota**: A senha é hashed usando PBKDF2-SHA256 com 10.000 iterações. A senha em texto claro nunca é armazenada no banco.

---

## Como Funciona

### Fluxo Automático

1. **Program.cs Startup**:
   ```csharp
   // Database initialization on application start
   using (var scope = app.Services.CreateScope())
   {
       var dbContext = scope.ServiceProvider.GetRequiredService<ZivroDbContext>();
       var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

       // Apply migrations
       await dbContext.Database.MigrateAsync();
       
       // Seed power user
       await SeedData.SeedAsync(dbContext, passwordHasher);
   }
   ```

2. **Verificação Inteligente**:
   - Se o usuário power user já existe, o seed é ignorado
   - Não há duplicação de dados
   - Seguro executar várias vezes

3. **Classe SeedData**:
   - Localizada em `Zivro.Infrastructure\Data\SeedData.cs`
   - Métodos estáticos para população de dados
   - Tratamento de exceções com logging

---

## Métodos Disponíveis

### 1. SeedData.SeedAsync()

Cria o usuário power user principal.

```csharp
await SeedData.SeedAsync(dbContext, passwordHasher);
```

**Comportamento**:
- Verifica se usuário com email "poweruser@zivro.com" já existe
- Se não existe, cria novo usuário com:
  - ID fixo: `00000000-0000-0000-0000-000000000001`
  - Senha hashed
  - Status ativo
  - Timestamp UTC
- Exibe mensagens de sucesso/erro no console

**Saída Console**:
```
✓ Power user created successfully.
  Email: poweruser@zivro.com
  Password: PowerUser@123456
  ID: 00000000-0000-0000-0000-000000000001
```

### 2. SeedData.SeedTestUsersAsync()

Cria um conjunto de usuários de teste (desenvolvimento apenas).

```csharp
await SeedData.SeedTestUsersAsync(dbContext, passwordHasher);
```

**Usuários Criados**:
1. **Regular User**
   - Email: `user@zivro.com`
   - Senha: `RegularUser@123`

2. **Test User**
   - Email: `test@zivro.com`
   - Senha: `TestUser@123`

3. **Demo User**
   - Email: `demo@zivro.com`
   - Senha: `DemoUser@123`

**Comportamento**:
- Verifica se usuários de teste já existem
- Cria IDs aleatórios para cada usuário
- Apenas adicionado em ambiente Development (comentado por padrão)

---

## Como Ativar Seed de Teste

Para adicionar usuários de teste em ambiente de desenvolvimento, descomente em `Program.cs`:

```csharp
// Uncomment to seed test users in development
if (app.Environment.IsDevelopment())
{
    await SeedData.SeedTestUsersAsync(dbContext, passwordHasher);
}
```

---

## Executar Migrações Manualmente

Se precisar criar ou aplicar migrações manualmente:

```bash
# Criar uma nova migração
dotnet ef migrations add <NomeMigração> --project Zivro.Infrastructure --startup-project Zivro.API

# Exemplo: Primeira migração
dotnet ef migrations add InitialCreate --project Zivro.Infrastructure --startup-project Zivro.API

# Aplicar migrações
dotnet ef database update --project Zivro.Infrastructure --startup-project Zivro.API

# Ver histórico de migrações
dotnet ef migrations list --project Zivro.Infrastructure --startup-project Zivro.API

# Reverter para migração específica
dotnet ef database update <NomeMigração> --project Zivro.Infrastructure --startup-project Zivro.API
```

**Nota**: As migrações são aplicadas automaticamente no Program.cs usando `dbContext.Database.MigrateAsync()`

---

## Estrutura de Arquivo

```
Zivro.Infrastructure/
└── Data/
    ├── SeedData.cs          ← Classe de seeding
    └── ZivroDbContext.cs    ← DbContext
```

---

## Esquema de Banco de Dados

### Tabela Users (criada por migração)

```sql
CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(255) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedAt] datetime2 NULL,
    [IsActive] bit NOT NULL DEFAULT (1),
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
    CONSTRAINT [IX_User_Email_Unique] UNIQUE ([Email])
);
```

### Tabela RefreshTokens (criada por migração)

```sql
CREATE TABLE [RefreshTokens] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [Token] nvarchar(500) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [IsRevoked] bit NOT NULL DEFAULT (0),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users] FOREIGN KEY ([UserId]) 
        REFERENCES [Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [IX_RefreshToken_Token] UNIQUE ([Token]),
    INDEX [IX_RefreshToken_UserId] ([UserId]),
    INDEX [IX_RefreshToken_ExpiresAt] ([ExpiresAt])
);
```

---

## Tratamento de Erros

O seeding inclui tratamento de exceções:

```csharp
try
{
    await dbContext.Database.MigrateAsync();
    await SeedData.SeedAsync(dbContext, passwordHasher);
}
catch (Exception ex)
{
    Console.WriteLine($"✗ Error during database initialization: {ex.Message}");
    if (app.Environment.IsDevelopment())
    {
        throw;
    }
}
```

**Comportamento**:
- **Desenvolvimento**: Exceção é lançada (para debug)
- **Produção**: Exceção é registrada mas aplicação continua

---

## Login com Power User

Usar este usuário para testes de autenticação:

```bash
# Exemplo com cURL
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "poweruser@zivro.com",
    "password": "PowerUser@123456"
  }'
```

---

## Resetar Banco de Dados

Para limpar e reinicializar o banco:

```bash
# Drop database (remove tudo)
dotnet ef database drop --project Zivro.Infrastructure --startup-project Zivro.API --force

# Recriar do zero (migrações + seed)
dotnet run (inicia aplicação com seeding automático)
```

---

## Extensão Futura

Para adicionar mais dados de seed, expanda a classe `SeedData.cs`:

```csharp
public static async Task SeedCompaniesAsync(ZivroDbContext context)
{
    // Implementar seed de empresas
}

public static async Task SeedProductsAsync(ZivroDbContext context)
{
    // Implementar seed de produtos
}
```

E chame em `Program.cs`:

```csharp
await SeedData.SeedAsync(dbContext, passwordHasher);
await SeedData.SeedCompaniesAsync(dbContext);
await SeedData.SeedProductsAsync(dbContext);
```

---

**Status**: ✅ Seed data configurado e funcional
**Última Atualização**: Build Release com sucesso
