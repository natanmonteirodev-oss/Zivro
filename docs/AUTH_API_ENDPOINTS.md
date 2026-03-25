# API de Autenticação - Documentação de Endpoints

## Overview

A API de autenticação oferece endpoints completos para registro, login, renovação de tokens e logout usando JWT access tokens e refresh tokens.

**Base URL**: `https://localhost:5001/api/auth` (desenvolvimento)  
**Autenticação**: Bearer Token (para endpoints protegidos futuros)  
**Content-Type**: `application/json`

---

## Endpoints

### 1. Registrar Novo Usuário

Cria uma nova conta de usuário com as credenciais fornecidas.

**Requisição**:
```http
POST /api/auth/register
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao@example.com",
  "password": "SenhaForte@123",
  "passwordConfirmation": "SenhaForte@123"
}
```

**Parâmetros**:
| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| name | string | ✅ | Nome completo do usuário (min: 3 caracteres) |
| email | string | ✅ | Email válido e único |
| password | string | ✅ | Senha com mínimo 6 caracteres |
| passwordConfirmation | string | ✅ | Confirmação da senha (deve coincidir) |

**Respostas**:

**200 OK - Sucesso**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "rT_KxZ9vL2pQ5mN8aBcDeFgHiJkLmNoPqRsT",
  "accessTokenExpiresAt": "2026-03-24T02:00:00Z",
  "refreshTokenExpiresAt": "2026-03-31T01:15:00Z",
  "tokenType": "Bearer"
}
```

**400 Bad Request - Validação falhou**:
```json
{
  "message": "Email já está registrado.",
  "code": "VALIDATION_ERROR",
  "timestamp": "2026-03-24T01:15:00Z"
}
```

**Exemplos de Erro de Validação**:
- Email já registrado
- Senhas não conferem
- Email inválido
- Senha muito fraca

---

### 2. Realizar Login

Autentica um usuário existente com email e senha.

**Requisição**:
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "joao@example.com",
  "password": "SenhaForte@123"
}
```

**Parâmetros**:
| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| email | string | ✅ | Email registrado |
| password | string | ✅ | Senha do usuário |

**Respostas**:

**200 OK - Login bem-sucedido**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "rT_KxZ9vL2pQ5mN8aBcDeFgHiJkLmNoPqRsT",
  "accessTokenExpiresAt": "2026-03-24T02:15:00Z",
  "refreshTokenExpiresAt": "2026-03-31T01:30:00Z",
  "tokenType": "Bearer"
}
```

**401 Unauthorized - Credenciais inválidas**:
```json
{
  "message": "Email ou senha inválidos.",
  "code": "INVALID_CREDENTIALS",
  "timestamp": "2026-03-24T01:20:00Z"
}
```

**400 Bad Request - Dados ausentes**:
```json
{
  "message": "Email e senha são obrigatórios.",
  "code": "EMPTY_CREDENTIALS",
  "timestamp": "2026-03-24T01:20:00Z"
}
```

**Casos de Erro**:
- Usuário não encontrado
- Senha incorreta
- Usuário inativo
- Email/senha vazios

---

### 3. Renovar Token de Acesso

Obtém um novo access token usando um refresh token válido.

**Requisição**:
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "rT_KxZ9vL2pQ5mN8aBcDeFgHiJkLmNoPqRsT"
}
```

**Parâmetros**:
| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| refreshToken | string | ✅ | Token obtido no login/registro |

**Respostas**:

**200 OK - Token renovado**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "rT_NewToken9vL2pQ5mN8aBcDeFgHiJkLmNoPqRsT",
  "accessTokenExpiresAt": "2026-03-24T03:15:00Z",
  "refreshTokenExpiresAt": "2026-03-31T02:30:00Z",
  "tokenType": "Bearer"
}
```

**401 Unauthorized - Token inválido ou expirado**:
```json
{
  "message": "Refresh token inválido ou expirado.",
  "code": "INVALID_TOKEN",
  "timestamp": "2026-03-24T01:25:00Z"
}
```

**400 Bad Request - Token ausente**:
```json
{
  "message": "Refresh token é obrigatório.",
  "code": "MISSING_TOKEN",
  "timestamp": "2026-03-24T01:25:00Z"
}
```

---

### 4. Realizar Logout (Revogar Token)

Invalida o refresh token, prevenindo seu uso futuro.

**Requisição**:
```http
POST /api/auth/logout
Content-Type: application/json

{
  "refreshToken": "rT_KxZ9vL2pQ5mN8aBcDeFgHiJkLmNoPqRsT"
}
```

**Parâmetros**:
| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| refreshToken | string | ✅ | Token a ser revogado |

**Respostas**:

**200 OK - Logout bem-sucedido**:
```json
{
  "message": "Logout realizado com sucesso.",
  "code": "LOGOUT_SUCCESS",
  "timestamp": "2026-03-24T01:30:00Z"
}
```

**401 Unauthorized - Token não encontrado**:
```json
{
  "message": "Token inválido ou já foi revogado.",
  "code": "INVALID_TOKEN",
  "timestamp": "2026-03-24T01:30:00Z"
}
```

**400 Bad Request - Token ausente**:
```json
{
  "message": "Refresh token é obrigatório.",
  "code": "MISSING_TOKEN",
  "timestamp": "2026-03-24T01:30:00Z"
}
```

---

### 5. Validar Credenciais

Verifica se um email/senha são válidos **sem gerar tokens** (útil para re-autenticação antes de operações sensíveis).

**Requisição**:
```http
POST /api/auth/validate
Content-Type: application/json

{
  "email": "joao@example.com",
  "password": "SenhaForte@123"
}
```

**Parâmetros**:
| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| email | string | ✅ | Email a validar |
| password | string | ✅ | Senha a validar |

**Respostas**:

**200 OK - Validação concluída**:
```json
{
  "isValid": true,
  "message": "Credenciais válidas.",
  "timestamp": "2026-03-24T01:35:00Z"
}
```

ou

```json
{
  "isValid": false,
  "message": "Credenciais inválidas.",
  "timestamp": "2026-03-24T01:35:00Z"
}
```

**400 Bad Request - Dados ausentes**:
```json
{
  "message": "Email e senha são obrigatórios.",
  "code": "EMPTY_CREDENTIALS",
  "timestamp": "2026-03-24T01:35:00Z"
}
```

---

## Fluxo de Autenticação Completo

### 1. Novo Usuário
```
POST /register
  ↓
200 OK com accessToken + refreshToken
  ↓
Armazenar tokens no cliente
  ↓
Usar accessToken em requisições autenticadas
```

### 2. Usuário Existente
```
POST /login
  ↓
200 OK com accessToken + refreshToken
  ↓
Armazenar tokens
  ↓
Acessar recursos protegidos com Bearer token
```

### 3. Token Expirado
```
POST /refresh com refreshToken anterior
  ↓
200 OK com novo accessToken + novo refreshToken
  ↓
Atualizar tokens locais
  ↓
Continuar com novo accessToken
```

### 4. Encerrar Sessão
```
POST /logout com refreshToken
  ↓
200 OK
  ↓
Limpar tokens do cliente
  ↓
Redirecionar para login
```

---

## Códigos de Status HTTP

| Status | Significado | Quando Ocorre |
|--------|-------------|---------------|
| 200 | OK | Operação bem-sucedida |
| 400 | Bad Request | Validação falhou, dados inválidos |
| 401 | Unauthorized | Credenciais inválidas, token expirado |
| 500 | Internal Server Error | Erro no servidor |

---

## Códigos de Erro (Error Code)

| Código | Descrição |
|--------|-----------|
| VALIDATION_ERROR | Dados não passaram na validação |
| INVALID_CREDENTIALS | Email/senha incorretos |
| EMPTY_CREDENTIALS | Email ou senha vazios |
| INVALID_REQUEST | Requisição malformada |
| MISSING_TOKEN | Refresh token não fornecido |
| INVALID_TOKEN | Token inválido, expirado ou revogado |
| REGISTRATION_ERROR | Erro ao registrar usuário |
| LOGIN_ERROR | Erro ao fazer login |
| REFRESH_ERROR | Erro ao renovar token |
| LOGOUT_ERROR | Erro ao fazer logout |

---

## Exemplo de Uso com cURL

### Registrar
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "name": "João Silva",
    "email": "joao@example.com",
    "password": "SenhaForte@123",
    "passwordConfirmation": "SenhaForte@123"
  }'
```

### Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "joao@example.com",
    "password": "SenhaForte@123"
  }'
```

### Refresh Token
```bash
curl -X POST https://localhost:5001/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "rT_KxZ9vL2pQ5mN8aBcDeFgHiJkLmNoPqRsT"
  }'
```

### Logout
```bash
curl -X POST https://localhost:5001/api/auth/logout \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "rT_KxZ9vL2pQ5mN8aBcDeFgHiJkLmNoPqRsT"
  }'
```

### Validar Credenciais
```bash
curl -X POST https://localhost:5001/api/auth/validate \
  -H "Content-Type: application/json" \
  -d '{
    "email": "joao@example.com",
    "password": "SenhaForte@123"
  }'
```

---

## Tempo de Expiração dos Tokens

| Token | Expiração | Renovável |
|-------|-----------|-----------|
| Access Token | 15 minutos | Sim (via refresh) |
| Refresh Token | 7 dias | Sim (novo gerado a cada refresh) |

---

## Próximos Passos

1. **Autenticação em Endpoints**: Proteger endpoints com `[Authorize]`
2. **JWT Secret Management**: Implementar gestão segura de secrets
3. **Rate Limiting**: Implementar rate limiting nos endpoints de auth
4. **Audit Logging**: Registrar todas as tentativas de acesso
5. **2FA**: Implementar autenticação de dois fatores
6. **Email Verification**: Verificação de email no registro

---

**Status**: ✅ Controllers implementados e compilando  
**Build**: Release/net8.0  
**Última atualização**: 24/03/2026
