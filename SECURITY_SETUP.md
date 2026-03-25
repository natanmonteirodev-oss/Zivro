# Configuration & Security Setup Guide

## ⚠️ Important: Sensitive Data Protection

This guide explains how to securely manage sensitive configuration data (passwords, API keys, JWT secrets, etc.) in the Zivro project.

---

## 🔐 Never Commit Sensitive Data

The following files are **EXCLUDED from git** via `.gitignore`:
- `appsettings.json` - Database connections, JWT secrets, email credentials
- `appsettings.*.json` - Environment-specific configs
- `.env` files - Environment variables
- `secrets.json` - User secrets
- `*.key`, `*.pem` - Encryption keys

---

## ✅ Setup Instructions

### 1. Create Local Configuration Files

Copy the example config to create your local configuration:

```bash
# Copy example to development appsettings
cp backend/src/Zivro.API/appsettings.json.example backend/src/Zivro.API/appsettings.json
```

### 2. Edit appsettings.json with Your Values

Update `backend/src/Zivro.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=Zivro;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  },
  "JWT": {
    "SecretKey": "YOUR_STRONG_256_BIT_SECRET_KEY_MIN_32_CHARACTERS_LONG",
    "Issuer": "Zivro.API",
    "Audience": "Zivro.App",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-specific-password",
    "UseSSL": true
  }
}
```

### 3. Generate a Strong JWT Secret Key

Use PowerShell to generate a secure 32+ character key:

```powershell
$bytes = [System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32)
[Convert]::ToBase64String($bytes)
```

Or use .NET's built-in secret manager:

```bash
cd backend/src/Zivro.API
dotnet user-secrets init
dotnet user-secrets set "JWT:SecretKey" "YOUR_GENERATED_SECRET_KEY"
```

### 4. Setup Database Connection

For SQL Server Local/Express:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.\\SQLEXPRESS01;Database=Zivro;Integrated Security=true;TrustServerCertificate=true;"
}
```

Or with explicit credentials:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=Zivro;User Id=sa;Password=YOUR_SA_PASSWORD;TrustServerCertificate=true;"
}
```

### 5. Setup Email (Optional)

#### Using Gmail:
1. Enable 2FA on your Google Account
2. Generate App-Specific Password: https://myaccount.google.com/apppasswords
3. Use in config:

```json
"Email": {
  "SenderEmail": "your-email@gmail.com",
  "SenderName": "Zivro Platform",
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "SmtpUsername": "your-email@gmail.com",
  "SmtpPassword": "your-app-password-here",
  "UseSSL": true
}
```

#### Using SendGrid:
1. Signup at https://sendgrid.com
2. Get API key from dashboard
3. Use in code with SendGridClient

---

## 📋 Checklist for Deployment

Before deploying to production:

- [ ] Generate new strong JWT secret key
- [ ] Use secure database password
- [ ] Configure real SMTP/Email service
- [ ] Set appropriate token expiry times
- [ ] Update Rate Limiting values if needed
- [ ] Set correct Issuer and Audience for JWT
- [ ] Use HTTPS/TLS certificates
- [ ] Enable HTTPS enforcement
- [ ] Rotate secrets regularly

---

## 🛡️ Security Best Practices

1. **Never commit appsettings.json** - It's in .gitignore for a reason
2. **Use different secrets per environment** - Dev, staging, production
3. **Rotate JWT secret keys** regularly
4. **Use strong passwords** - Min 16+ characters with mixed case, numbers, symbols
5. **Use environment variables** in production (not config files)
6. **Restrict file permissions** - Only app should read config files
7. **Monitor access logs** - Track authentication attempts
8. **Use HTTPS only** - Never send tokens over HTTP
9. **Enable audit logging** - Review security events regularly
10. **Keep dependencies updated** - Security patches are critical

---

## 🌐 Environment-Specific Configuration

For different environments, you can create:

- `appsettings.Development.json` - Dev environment secrets
- `appsettings.Staging.json` - Staging environment secrets
- `appsettings.Production.json` - Production secrets (keep secure!)

ASP.NET Core loads the appropriate file based on `ASPNETCORE_ENVIRONMENT`.

---

## 📝 Common Configuration Values

| Setting | Development | Production |
|---------|-------------|------------|
| JWT Expiry | 15 minutes | 15 minutes |
| Refresh Expiry | 7 days | 7 days |
| Login Rate Limit | 10/hour | 10/hour |
| Register Rate Limit | 5/hour | 5/hour |
| Email Verify Rate Limit | 20/hour | 20/hour |
| Log Level | Information | Warning |

---

## 🔧 Troubleshooting

**"Configuration key not found"**
- Ensure `appsettings.json` exists in the correct location
- Check JSON syntax is valid
- Verify Environment variable is set correctly

**"Unable to connect to database"**
- Check connection string in appsettings.json
- Verify SQL Server is running
- Check username and password are correct

**"Email not sending"**
- Verify SMTP credentials are correct
- Check firewall allows port 587/465
- Ensure app-specific passwords for Gmail
- Enable "Less secure apps" if using Gmail without 2FA

---

## 📚 Additional Resources

- [ASP.NET Core Configuration Documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
- [OWASP Secrets Management](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [SQL Server Connection Strings](https://www.connectionstrings.com/sql-server/)
