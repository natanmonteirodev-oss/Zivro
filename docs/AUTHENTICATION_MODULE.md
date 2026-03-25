# Authentication Module Documentation

## Overview

Complete authentication system for Zivro API built with .NET 8.0, Entity Framework Core 8, and SQL Server. Implements Clean Architecture with secure password hashing (PBKDF2-SHA256) and JWT-ready token management.

**Build Status**: ✅ All 7 projects compile successfully (Release/net8.0)

---

## Architecture Layers

### 1. Domain Layer (Zivro.Domain)
**Purpose**: Enterprise business rules and domain models

#### Entities
- **User.cs**
  - Properties: `Id` (Guid), `Name`, `Email` (unique), `PasswordHash`, `IsActive`, `CreatedAt`, `UpdatedAt`
  - Collections: `RefreshTokens` (one-to-many)
  - Methods: `IsValidUser()` - validates entity state
  - Inherits: `Entity` base class for audit fields

- **RefreshToken.cs**
  - Properties: `Id` (Guid), `UserId` (FK), `Token` (unique), `ExpiresAt`, `IsRevoked`, `CreatedAt`
  - Navigation: `User` property
  - Methods:
    - `IsValid()` - checks if token is not revoked and not expired
    - `Revoke()` - marks token as revoked with timestamp
  
- **Entity.cs** (Base Class)
  - Properties: `Id`, `CreatedAt` (UTC), `UpdatedAt` (nullable), `IsActive`
  - Implements equality based on Guid

#### Interfaces
- **IPasswordHasher.cs**
  - `Hash(string password)` → hashed password string
  - `Verify(string password, string hash)` → boolean
  - Abstraction allows easy replacement with BCrypt, Argon2, etc.

- **IAuthRepository.cs**
  - 8 async methods for CRUD and query operations
  - Methods:
    - `GetUserByEmailAsync(string email)` → User
    - `GetUserByIdAsync(Guid userId)` → User
    - `CreateUserAsync(User user)` → void
    - `CreateRefreshTokenAsync(RefreshToken token)` → void
    - `GetRefreshTokenAsync(string token)` → RefreshToken
    - `GetValidRefreshTokensAsync(Guid userId)` → IEnumerable<RefreshToken>
    - `RevokeRefreshTokenAsync(string token)` → void
    - `SaveChangesAsync()` → void

---

### 2. Application Layer (Zivro.Application)
**Purpose**: Use cases, business logic orchestration, DTOs

#### Data Transfer Objects (DTOs)
- **RegisterRequest.cs**
  - `Name` (string, required)
  - `Email` (string, required)
  - `Password` (string, required, min 6 chars)
  - `PasswordConfirmation` (string, required)

- **LoginRequest.cs**
  - `Email` (string, required)
  - `Password` (string, required)

- **AuthResponse.cs**
  - `AccessToken` (string) - JWT token (15-minute expiration)
  - `RefreshToken` (string) - Secure random token (7-day expiration)
  - `AccessTokenExpiresAt` (DateTime)
  - `RefreshTokenExpiresAt` (DateTime)
  - `TokenType` (string) - "Bearer"

#### Services
- **IAuthService.cs** - Interface defining authentication operations
- **AuthService.cs** - Complete implementation with 5 public methods:

  1. **RegisterAsync(RegisterRequest request)**: AuthResponse
     - Validates request (email unique, password requirements)
     - Hashes password using IPasswordHasher
     - Creates and persists User entity
     - Generates access + refresh tokens
     - Returns AuthResponse with new tokens

  2. **LoginAsync(LoginRequest request)**: AuthResponse
     - Retrieves user by email (case-insensitive)
     - Verifies password against stored hash
     - Validates user IsActive flag
     - Generates new tokens
     - Returns AuthResponse

  3. **RefreshTokenAsync(string refreshToken)**: AuthResponse
     - Validates refresh token exists and is valid (not revoked, not expired)
     - Revokes old refresh token
     - Generates new token pair
     - Returns AuthResponse with new tokens

  4. **RevokeTokenAsync(string refreshToken)**: void
     - Logout operation
     - Marks specified refresh token as revoked

  5. **ValidateUserAsync(string email, string password)**: bool
     - Credential validation without token generation
     - Useful for non-auth scenarios (password change verification)

  **Token Configuration**:
  - Access Token Expiration: 15 minutes
  - Refresh Token Expiration: 7 days

  **Error Handling**:
  - `InvalidOperationException`: Business logic violations (duplicate email, invalid data)
  - `UnauthorizedAccessException`: Authentication failures (invalid password, user not found)
  - `ArgumentNullException`: Missing required inputs

---

### 3. Infrastructure Layer (Zivro.Infrastructure)
**Purpose**: Data persistence, external services, security implementations

#### Security
- **Sha256PasswordHasher.cs** - Implements IPasswordHasher
  
  **Algorithm**: PBKDF2-HMACSHA256
  - Iterations: 10,000
  - Salt Size: 16 bytes
  - Derived Key Size: 32 bytes
  - Hash Function: SHA256

  **Implementation Details**:
  ```
  Hash Process:
  1. Generate random 16-byte salt
  2. Use PBKDF2 to derive 32-byte hash from password
  3. Concatenate salt + hash
  4. Return as Base64 string
  
  Verify Process:
  1. Decode Base64 hash
  2. Extract salt (first 16 bytes)
  3. Regenerate hash with same salt
  4. Constant-time comparison of hash bytes
  ```

  **Modernization**: Uses `RandomNumberGenerator.Fill()` (EF Core 8 compatible) instead of obsolete `RNGCryptoServiceProvider`

  **Extensibility**: Replace implementation in DI container to use BCrypt.Net, Argon2, or other algorithms without code changes

#### Data Persistence
- **ZivroDbContext.cs** - Entity Framework Core DbContext

  **DbSets**:
  - `Users` - Table: `[Users]`
  - `RefreshTokens` - Table: `[RefreshTokens]`

  **Fluent API Configuration**:
  
  User Entity:
  - Primary Key: `Id` (Guid)
  - Required Fields: `Name` (max 255), `Email` (max 255), `PasswordHash`
  - Default Fields: `CreatedAt` (SQL UTC), `UpdatedAt` (nullable), `IsActive` (true)
  - Unique Index: `IX_User_Email_Unique` on Email (case-sensitive in SQL)
  - Relationships: One-to-Many with RefreshTokens (cascade delete)

  RefreshToken Entity:
  - Primary Key: `Id` (Guid)
  - Foreign Key: `UserId` → User (cascade delete)
  - Required Fields: `Token` (max 500), `ExpiresAt`
  - Default Fields: `IsRevoked` (false), `CreatedAt`
  - Unique Index: `IX_RefreshToken_Token` on Token
  - Performance Indexes: `/IX_RefreshToken_UserId/`, `/IX_RefreshToken_ExpiresAt/`

  **SQL Defaults**:
  - Timestamp fields auto-populated via `GETUTCDATE()` in SQL Server
  - Reduces ORM overhead for timestamp generation

#### Repository Pattern
- **AuthRepository.cs** - Implements IAuthRepository

  **Query Methods**:
  - Case-insensitive email lookups with `ToLowerInvariant()`
  - SQL indexes leveraged for performance (Email, UserId, ExpiresAt, Token)
  - `AsNoTracking()` for read-only operations (efficient memory usage)

  **Write Methods**:
  - `AddAsync()` pattern for single-pass inserts
  - `SaveChangesAsync()` delegates to DbContext
  - Token revocation loads tracked entity to update state

  **Performance Considerations**:
  - Email queries indexed for O(1) lookup
  - Token expiration queries optimized with ExpiresAt index
  - Batch operations use SaveChangesAsync once

---

### 4. API Layer (Zivro.API)
**Current State**: Basic infrastructure ready, auth endpoints pending

- **Program.cs**: ASP.NET Core setup with Swagger, CORS, health checks
- **Controllers/HealthController.cs**: Operational validation endpoint
- **TODO**: AuthController with Register, Login, Refresh, Logout endpoints

---

## Token Flow Diagrams

### Registration Flow
```
POST /api/auth/register
    ↓
RegisterRequest validation
    ↓
Email uniqueness check
    ↓
Password hashing (PBKDF2-SHA256)
    ↓
User entity creation
    ↓
GenerateAccessToken() [15 min placeholder JWT]
GenerateRefreshToken() [7 day secure random]
    ↓
AuthResponse returned
```

### Login Flow
```
POST /api/auth/login
    ↓
User lookup by email
    ↓
Password verification
    ↓
IsActive status check
    ↓
Token generation
    ↓
RefreshToken persisted to database
    ↓
AuthResponse returned
```

### Token Refresh Flow
```
POST /api/auth/refresh
    ↓
Validate refresh token exists
    ↓
Check token not revoked + not expired
    ↓
Revoke old token
    ↓
Generate new access + refresh tokens
    ↓
Persist new RefreshToken
    ↓
AuthResponse returned
```

### Logout Flow
```
POST /api/auth/logout
    ↓
Find refresh token by value
    ↓
Mark as revoked (IsRevoked = true)
    ↓
Persist to database
    ↓
Client discards tokens locally
```

---

## Database Schema

### Users Table
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

### RefreshTokens Table
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

## Dependency Injection Configuration

**Pending Implementation** - Add to `Program.cs` or CrossCutting extension:

```csharp
// Security
services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();

// Database
services.AddDbContext<ZivroDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
);

// Repositories & Services
services.AddScoped<IAuthRepository, AuthRepository>();
services.AddScoped<IAuthService, AuthService>();
```

---

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=Zivro;Integrated Security=true;TrustServerCertificate=true;"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-chars-long-required-for-hs256",
    "Issuer": "https://zivro.com",
    "Audience": "zivro-client",
    "ExpirationMinutes": 15
  }
}
```

### global.json (Forces .NET 8.0.419)
```json
{
  "sdk": {
    "version": "8.0.419"
  }
}
```

---

## Current Implementation Status

| Component | Status | Details |
|-----------|--------|---------|
| Domain Entities | ✅ Complete | User, RefreshToken, Entity base class |
| Domain Interfaces | ✅ Complete | IPasswordHasher, IAuthRepository |
| Application DTOs | ✅ Complete | Register/Login/AuthResponse |
| Password Hasher | ✅ Complete | PBKDF2-SHA256 with 10k iterations |
| AuthService | ✅ Complete | All 5 async methods + validation |
| DbContext | ✅ Complete | Fluent API with indexes and relationships |
| AuthRepository | ✅ Complete | All CRUD and query operations |
| Build Validation | ✅ Complete | 0 errors, Release/net8.0 binaries generated |
| API Endpoints | ❌ Pending | AuthController not yet created |
| JWT Generation | ❌ Pending | Placeholder implemented, needs System.IdentityModel.Tokens.Jwt |
| DI Configuration | ❌ Pending | Services not registered in Program.cs |
| EF Migrations | ❌ Pending | Database schema not created |
| Unit Tests | ❌ Pending | Test projects created but empty |

---

## Next Steps

### Phase 1: Immediate (Blocking)
1. **Implement JWT Token Generation**
   - Install `System.IdentityModel.Tokens.Jwt` NuGet package
   - Create `JwtOptions` class in CrossCutting
   - Update `AuthService.GenerateAccessToken()` to create JwtSecurityToken
   - Add JWT configuration to appsettings.json

2. **Configure Dependency Injection**
   - Register services in Program.cs
   - Add DbContext configuration with connection string
   - Validate DI container on application startup

3. **Create API Endpoints**
   - Create `AuthController` class
   - Implement POST /api/auth/register
   - Implement POST /api/auth/login
   - Implement POST /api/auth/refresh
   - Implement POST /api/auth/logout
   - Add response handling (200 Success, 400 Bad Request, 401 Unauthorized)

### Phase 2: Database
1. Create Entity Framework migrations
2. Initialize database schema
3. Test CRUD operations

### Phase 3: Testing & Security
1. Unit tests for AuthService
2. Unit tests for PasswordHasher
3. Integration tests for API endpoints
4. Add authentication middleware/filters
5. Implement rate limiting on auth endpoints

### Phase 4: Extended Features
1. Email verification for registration
2. Password reset flow
3. Two-factor authentication
4. Role-based access control (RBAC)
5. OAuth/OpenID Connect integration

---

## Security Notes

- **Password Storage**: Never store passwords in plain text. Current PBKDF2 implementation is industry-standard and suitable for most applications.
- **BCrypt Migration**: To switch to BCrypt, create `BcryptPasswordHasher : IPasswordHasher` and change DI registration (no code changes needed elsewhere).
- **JWT Secrets**: Store secret keys in secure configuration (Azure Key Vault, HashiCorp Vault, environment variables).
- **HTTPS**: All endpoints must use HTTPS in production.
- **Token Rotation**: Refresh tokens should be rotated on each use (current implementation doesn't require this, but pattern is prepared).
- **CORS**: Configure CORS appropriately to prevent token leakage to untrusted origins.

---

## Build Command

```powershell
# Full rebuild
dotnet build --configuration Release

# Debug build
dotnet build

# Clean and rebuild
dotnet clean
dotnet build
```

**Current Build Status**: All 7 projects compile successfully in 0.90 seconds.

---

## Project References

- `Zivro.Domain` ← No dependencies (foundational)
- `Zivro.Application` → `Zivro.Domain`
- `Zivro.Infrastructure` → `Zivro.Domain`, `Zivro.Application`
- `Zivro.CrossCutting` → `Zivro.Infrastructure`
- `Zivro.API` → All lower layers
- `Zivro.UnitTests` → `Zivro.Application`, `Zivro.Infrastructure`
- `Zivro.IntegrationTests` → `Zivro.Infrastructure`

---

## Key Files Structure

```
backend/src/
├── Zivro.Domain/
│   ├── Entities/User.cs
│   ├── Entities/RefreshToken.cs
│   ├── Entities/Entity.cs
│   └── Interfaces/
│       ├── IPasswordHasher.cs
│       └── IAuthRepository.cs
│
├── Zivro.Application/
│   ├── DTO/Auth/
│   │   ├── RegisterRequest.cs
│   │   ├── LoginRequest.cs
│   │   └── AuthResponse.cs
│   ├── Interfaces/IAuthService.cs
│   └── Services/AuthService.cs
│
├── Zivro.Infrastructure/
│   ├── Security/Sha256PasswordHasher.cs
│   ├── Data/ZivroDbContext.cs
│   └── Repositories/AuthRepository.cs
│
└── Zivro.API/
    ├── Program.cs
    ├── Controllers/HealthController.cs
    └── Controllers/AuthController.cs [TODO]
```

---

**Last Updated**: After successful Release build (net8.0)
**Compilation Status**: ✅ 0 Errors, 5 Harmless Warnings
