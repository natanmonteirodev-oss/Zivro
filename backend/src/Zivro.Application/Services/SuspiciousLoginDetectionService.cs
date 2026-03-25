namespace Zivro.Application.Services;

using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using Zivro.Application.Interfaces;
using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;

/// <summary>
/// Serviço de detecção de logins suspeitos
/// </summary>
public class SuspiciousLoginDetectionService : ISuspiciousLoginDetectionService
{
    private readonly ISuspiciousLoginRepository _suspiciousLoginRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<SuspiciousLoginDetectionService> _logger;

    // IPs conhecidos como seguros (pode ser expandido)
    private readonly HashSet<string> _trustedIps = new();

    public SuspiciousLoginDetectionService(
        ISuspiciousLoginRepository suspiciousLoginRepository,
        IAuthRepository authRepository,
        IAuditLogService auditLogService,
        ILogger<SuspiciousLoginDetectionService> logger)
    {
        _suspiciousLoginRepository = suspiciousLoginRepository;
        _authRepository = authRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<SuspiciousLogin?> AnalyzeLoginAsync(Guid? userId, string email, string ipAddress, string userAgent)
    {
        // Se é login novo ou sem user ID, não analisa
        if (userId == null)
            return null;

        var user = await _authRepository.GetUserByIdAsync(userId.Value);
        if (user == null)
            return null;

        var suspiciousLogins = new List<(SuspiciousLoginReason, string?)>();

        // 1. Verificar nova localização
        var locationCheck = await CheckNewLocationAsync(userId.Value, ipAddress);
        if (locationCheck.IsSuspicious)
        {
            suspiciousLogins.Add((SuspiciousLoginReason.NewLocation, locationCheck.Description));
        }

        // 2. Verificar novo dispositivo
        var deviceFingerprint = GenerateDeviceFingerprint(userAgent);
        var deviceCheck = await CheckNewDeviceAsync(userId.Value, deviceFingerprint);
        if (deviceCheck.IsSuspicious)
        {
            suspiciousLogins.Add((SuspiciousLoginReason.NewDevice, deviceCheck.Description));
        }

        // 3. Verificar velocidade de login
        var velocityCheck = await CheckVelocityAsync(userId.Value, ipAddress);
        if (velocityCheck.IsSuspicious)
        {
            suspiciousLogins.Add((SuspiciousLoginReason.VelocityCheck, velocityCheck.Description));
        }

        // 4. Verificar horário anormal
        var timeCheck = CheckAnomalousTime(user);
        if (timeCheck.IsSuspicious)
        {
            suspiciousLogins.Add((SuspiciousLoginReason.AnomalousTime, timeCheck.Description));
        }

        // 5. Detectar VPN/Proxy
        var proxyCheck = DetectProxy(ipAddress, userAgent);
        if (proxyCheck.IsSuspicious)
        {
            suspiciousLogins.Add((SuspiciousLoginReason.ProxyDetected, proxyCheck.Description));
        }

        // Se nenhum problema encontrado
        if (suspiciousLogins.Count == 0)
            return null;

        // Cria registro de login suspeito
        var suspiciousLogin = new SuspiciousLogin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Reason = suspiciousLogins.First().Item1,
            ReasonDescription = string.Join("; ", suspiciousLogins.Select(x => x.Item2)),
            DeviceFingerprint = deviceFingerprint,
            DetectedAt = DateTime.UtcNow
        };

        await _suspiciousLoginRepository.CreateAsync(suspiciousLogin);

        // Notifica usuário
        await NotifyUserOfSuspiciousLoginAsync(user, suspiciousLogin);

        _logger.LogWarning("Suspicious login detected for user {UserId}: {Reason}", userId, suspiciousLogin.Reason);
        return suspiciousLogin;
    }

    public async Task<List<SuspiciousLogin>> GetSuspiciousLoginsAsync(Guid userId, int limit = 10)
    {
        return await _suspiciousLoginRepository.GetByUserIdAsync(userId, limit);
    }

    public async Task ConfirmSuspiciousLoginAsync(Guid suspiciousLoginId, bool wasLegitimate)
    {
        var suspiciousLogin = await _suspiciousLoginRepository.GetByIdAsync(suspiciousLoginId);
        if (suspiciousLogin == null)
            return;

        if (wasLegitimate)
        {
            // Marca como legítimo e confia neste IP/Device temporariamente
            _trustedIps.Add(suspiciousLogin.IpAddress);
            _logger.LogInformation("Suspicious login {Id} confirmed as legitimate", suspiciousLoginId);
        }
        else
        {
            // Usuário confirmou que não foi ele - pode ativar medidas adicionais
            // TODO: Implementar força reset de password, força logout de todos os dispositivos
            _logger.LogInformation("Suspicious login {Id} confirmed as fraudulent - user action required", suspiciousLoginId);
        }
    }

    public async Task<(string? Country, string? City, double? Latitude, double? Longitude)> GetLocationFromIpAsync(string ipAddress)
    {
        // TODO: Integrar com serviço real como MaxMind GeoIP2, IPStack, etc
        // Por agora, simulamos
        await Task.Delay(10);

        // Simulação simples
        return ("Brazil", "São Paulo", -23.5505, -46.6333);
    }

    public string GenerateDeviceFingerprint(string userAgent)
    {
        var hash = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(userAgent));
        return Convert.ToBase64String(hash);
    }

    private async Task<(bool IsSuspicious, string? Description)> CheckNewLocationAsync(Guid userId, string ipAddress)
    {
        var (country, city, _, _) = await GetLocationFromIpAsync(ipAddress);
        
        // Obtém logins recentes do usuário
        var recentLogins = await _suspiciousLoginRepository.GetByUserIdAsync(userId, 10);
        
        // Se é primeiro login ou não tem nenhum recente
        if (!recentLogins.Any(x => x.Country != null))
            return (false, null);

        // Verifica se é uma localização completamente nova
        var differentCountries = recentLogins.Where(x => x.Country != country).ToList();
        if (differentCountries.Count > 0 && recentLogins.Count >= 5)
        {
            return (true, $"New country detected: {country}");
        }

        return (false, null);
    }

    private async Task<(bool IsSuspicious, string? Description)> CheckNewDeviceAsync(Guid userId, string deviceFingerprint)
    {
        var recentLogins = await _suspiciousLoginRepository.GetByUserIdAsync(userId, 10);
        
        if (recentLogins.Count < 2)
            return (false, null);

        var knownDevices = recentLogins.Select(x => x.DeviceFingerprint).Distinct().Count();
        var isNewDevice = !recentLogins.Any(x => x.DeviceFingerprint == deviceFingerprint);

        if (isNewDevice && knownDevices >= 3)
        {
            return (true, "New device detected");
        }

        return (false, null);
    }

    private async Task<(bool IsSuspicious, string? Description)> CheckVelocityAsync(Guid userId, string ipAddress)
    {
        var recentLogin = await _suspiciousLoginRepository.GetRecentByIpAndEmailAsync(ipAddress, "", 5);
        
        if (recentLogin != null)
        {
            var timeSinceLastLogin = DateTime.UtcNow - recentLogin.DetectedAt;
            if (timeSinceLastLogin.TotalMinutes < 1)
            {
                return (true, "Login from same IP within 1 minute");
            }
        }

        return (false, null);
    }

    private (bool IsSuspicious, string? Description) CheckAnomalousTime(User user)
    {
        var hourNow = DateTime.UtcNow.Hour;
        
        // Se login fora do horário normal (3am-6am é anômalo para a maioria)
        if (hourNow >= 3 && hourNow <= 6)
        {
            return (true, "Login at unusual hour");
        }

        return (false, null);
    }

    private (bool IsSuspicious, string? Description) DetectProxy(string ipAddress, string userAgent)
    {
        // Simples detecção baseada em padrões conhecidos
        var proxyIndicators = new[] { "proxy", "vpn", "tor", "anonymize", "hide" };
        var hasProxyIndicator = proxyIndicators.Any(x => userAgent.Contains(x, StringComparison.OrdinalIgnoreCase));

        // Também pode usar MaxMind ASN para detectar proxies/VPNs
        // TODO: Integrar com API real

        if (hasProxyIndicator)
        {
            return (true, "VPN or proxy detected");
        }

        return (false, null);
    }

    private async Task NotifyUserOfSuspiciousLoginAsync(User user, SuspiciousLogin suspiciousLogin)
    {
        try
        {
            var subject = "Suspicious Login Detected";
            var htmlContent = $@"
                <h2>Security Alert: Suspicious Login</h2>
                <p>Hi {user.Name},</p>
                <p>We detected a login to your account from an unusual location or device:</p>
                <ul>
                    <li>IP Address: {suspiciousLogin.IpAddress}</li>
                    <li>Location: {suspiciousLogin.City}, {suspiciousLogin.Country}</li>
                    <li>Time: {suspiciousLogin.DetectedAt:yyyy-MM-dd HH:mm:ss}</li>
                    <li>Reason: {suspiciousLogin.ReasonDescription}</li>
                </ul>
                <p>If this was you, you can safely ignore this email.</p>
                <p>If this wasn't you, please secure your account by changing your password immediately.</p>
            ";

            // TODO: Implementar envio de email real
            suspiciousLogin.UserNotified = true;
            suspiciousLogin.NotifiedAt = DateTime.UtcNow;
            await _suspiciousLoginRepository.UpdateAsync(suspiciousLogin);

            System.Diagnostics.Debug.WriteLine($"[DEV] Suspicious login notification sent to {user.Email}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user {UserId} of suspicious login", user.Id);
        }
    }
}
