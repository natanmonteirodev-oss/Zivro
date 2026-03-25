namespace Zivro.Application.Interfaces;

using Zivro.Domain.Entities;

/// <summary>
/// Serviço para detectar e notificar logins suspeitos
/// </summary>
public interface ISuspiciousLoginDetectionService
{
    /// <summary>
    /// Analisa um login e determina se é suspeito
    /// </summary>
    Task<SuspiciousLogin?> AnalyzeLoginAsync(Guid? userId, string email, string ipAddress, string userAgent);

    /// <summary>
    /// Obtém lista de logins suspeitos de um usuário
    /// </summary>
    Task<List<SuspiciousLogin>> GetSuspiciousLoginsAsync(Guid userId, int limit = 10);

    /// <summary>
    /// Marca um login suspeito como confirmado/rejeitado pelo usuário
    /// </summary>
    Task ConfirmSuspiciousLoginAsync(Guid suspiciousLoginId, bool wasLegitimate);

    /// <summary>
    /// Faz lookup de geolocalização por IP
    /// </summary>
    Task<(string? Country, string? City, double? Latitude, double? Longitude)> GetLocationFromIpAsync(string ipAddress);

    /// <summary>
    /// Gera fingerprint do dispositivo baseado em user agent
    /// </summary>
    string GenerateDeviceFingerprint(string userAgent);
}
