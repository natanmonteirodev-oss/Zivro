# Swagger/OpenAPI com Autenticação JWT

## Overview

O Swagger (via Swashbuckle) está configurado para exibir e documentar todos os endpoints da API com suporte completo a autenticação Bearer Token (JWT).

**Acesso**: `https://localhost:5001/swagger` (desenvolvimento)  
**OpenAPI JSON**: `https://localhost:5001/swagger/v1/swagger.json`

---

## Configuração Implementada

### 1. Security Scheme (Bearer Token)

Definido no `Program.cs`:

```csharp
var securityScheme = new OpenApiSecurityScheme
{
    Name = "JWT Authentication",
    Description = "JWT Authorization header using the Bearer scheme",
    In = ParameterLocation.Header,
    Type = SecuritySchemeType.Http,
    Scheme = "bearer",
    BearerFormat = "JWT"
};
```

### 2. Security Requirement

Todos os endpoints por padrão requerem autenticação:

```csharp
var securityRequirement = new OpenApiSecurityRequirement
{
    { securityScheme, Array.Empty<string>() }
};

options.AddSecurityRequirement(securityRequirement);
```

### 3. Endpoints Sem Autenticação

Os seguintes endpoints estão marcados com `[AllowAnonymous]`:

- ✅ POST `/api/auth/register` - Registro de novo usuário
- ✅ POST `/api/auth/login` - Login
- ✅ POST `/api/auth/refresh` - Renovar token
- ✅ POST `/api/auth/logout` - Logout
- ✅ POST `/api/auth/validate` - Validar credenciais
- ✅ GET `/api/health` - Health check (sem modificação de docs)

---

## Como Usar no Swagger UI

### 1. Obter Token

1. Clique no endpoint **POST /api/auth/login**
2. Clique em **"Try it out"**
3. Preencha o corpo da requisição:
   ```json
   {
     "email": "poweruser@zivro.com",
     "password": "PowerUser@123456"
   }
   ```
4. Clique **"Execute"**
5. Copie o valor de `accessToken` da resposta

### 2. Autenticar no Swagger

1. Clique no botão **"Authorize"** (no topo da página do Swagger)
2. Cole o token no formato: `Bearer <seu_token_aqui>`
   
   Exemplo:
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

3. Clique em **"Authorize"**

### 3. Testar Endpoints Protegidos

Agora qualquer endpoint que requeira autenticação incluirá automaticamente o header:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Endpoints do Swagger

### Endpoints Públicos (Sem Token)

| Método | Endpoint | Status | Descrição |
|--------|----------|--------|-----------|
| GET | `/api/health` | ✅ | Health check |
| POST | `/api/auth/register` | ✅ | Registrar novo usuário |
| POST | `/api/auth/login` | ✅ | Fazer login |
| POST | `/api/auth/refresh` | ✅ | Renovar token |
| POST | `/api/auth/logout` | ✅ | Logout |
| POST | `/api/auth/validate` | ✅ | Validar credenciais |

### Endpoints Protegidos (Requerem Token)

Qualquer endpoint futuro por padrão será protegido. Use `[AllowAnonymous]` para exceções.

---

## Documentação XML

A API gera documentação XML a partir dos comentários de código:

```xml
<!-- Resultado em bin/Release/net8.0/Zivro.API.xml -->
<member name="T:Zivro.API.Controllers.AuthController">
  <summary>
    Controller responsável pela autenticação e autorização de usuários.
  </summary>
</member>
```

Esta documentação é automaticamente incluída no Swagger UI.

---

## Metadados da API

Configurados no `Program.cs`:

```csharp
options.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "Zivro API",
    Version = "v1",
    Description = "API de autenticação e autorização da plataforma Zivro",
    Contact = new OpenApiContact
    {
        Name = "Zivro Team",
        Email = "dev@zivro.com"
    },
    License = new OpenApiLicense
    {
        Name = "MIT License"
    }
});
```

---

## Exemplo: Flow Completo no Swagger

### Passo 1: Registrar Novo Usuário

```bash
POST /api/auth/register
Content-Type: application/json

{
  "name": "Maria Silva",
  "email": "maria@example.com",
  "password": "SenhaSegura@123",
  "passwordConfirmation": "SenhaSegura@123"
}
```

**Resposta 200:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "rT_KxZ9vL2pQ5mN8aBcDeFgHiJkLmNoPqRsT",
  "accessTokenExpiresAt": "2026-03-24T02:30:00Z",
  "refreshTokenExpiresAt": "2026-03-31T02:15:00Z",
  "tokenType": "Bearer"
}
```

### Passo 2: Autorizar no Swagger

1. Clique **Authorize** (canto superior direito)
2. Cole no campo do valor:
   ```
   Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```
3. Clique **Authorize**

### Passo 3: Usar Token em Outros Endpoints

Agora o token será automaticamente enviado em todas as requisições (quando o endpoint requer autenticação).

---

## Configuração de Ambiente

### Development
```bash
# Swagger habilitado
https://localhost:5001/swagger
```

### Production
```bash
# Swagger desabilitado por padrão
# Configure conforme necessário em Program.cs
```

---

## Próximas Implementações

### 1. JWT Real

Quando a geração real de JWT estiver implementada:

```csharp
options.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
```

### 2. Mais Ambientes

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "Zivro API - Development";
    });
}
```

### 3. OAuth 2.0 / OpenID Connect

```csharp
options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
{
    Type = SecuritySchemeType.OAuth2,
    Flows = new OpenApiOAuthFlows
    {
        AuthorizationCode = new OpenApiOAuthFlow
        {
            AuthorizationUrl = new Uri("https://auth.example.com/authorize"),
            TokenUrl = new Uri("https://auth.example.com/token"),
            Scopes = new Dictionary<string, string>
            {
                { "api", "Access API" }
            }
        }
    }
});
```

---

## Troubleshooting

### Token não aparece no Header

- ✅ Clique **Authorize** e preencha corretamente
- ✅ Use o formato `Bearer <token>`
- ✅ Certifique-se de que o endpoint é protegido

### "401 Unauthorized" em Todos os Endpoints

- ✅ Token expirou - faça login novamente
- ✅ Token inválido - verifique o formato
- ✅ JWT validation não está implementada (TODO)

### Documentação XML Não Aparece

- ✅ Recompile o projeto (Release mode)
- ✅ Limpe a cache do navegador
- ✅ Verifique se `GenerateDocumentationFile=true` está no .csproj

---

## Referências

- [Swashbuckle.AspNetCore Documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
- [OpenAPI 3.0 Specification](https://spec.openapis.org/oas/v3.0.3)
- [JWT Overview](https://jwt.io/)
- [Bearer Token Usage](https://tools.ietf.org/html/rfc6750)

---

**Status**: ✅ Swagger com autenticação JWT configurado  
**Última atualização**: 24/03/2026  
**Build**: Release/net8.0
