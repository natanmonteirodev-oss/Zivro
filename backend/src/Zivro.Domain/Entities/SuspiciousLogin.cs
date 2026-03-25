namespace Zivro.Domain.Entities;

/// <summary>
/// Representa uma tentativa de login suspeita
/// </summary>
public class SuspiciousLogin
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Email { get; set; } = default!;
    public string IpAddress { get; set; } = default!;
    public string UserAgent { get; set; } = default!;
    public string? Country { get; set; }
    public string? City { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? DeviceFingerprint { get; set; }
    public SuspiciousLoginReason Reason { get; set; }
    public string? ReasonDescription { get; set; }
    public bool UserNotified { get; set; }
    public DateTime NotifiedAt { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
}

/// <summary>
/// Razões que classificam um login como suspeito
/// </summary>
public enum SuspiciousLoginReason
{
    NewLocation,           // Nova localização geográfica
    NewDevice,             // Novo dispositivo baseado em fingerprint
    RareCombination,       // Combinação rara de localização + dispositivo
    VelocityCheck,         // Login muito rápido após outro login
    BruteForcePrevention,  // Muitas tentativas falhadas
    AnomalousTime,         // Login em horário anormal
    ProxyDetected,         // Detectado uso de proxy/VPN
    BotBehavior            // Comportamento suspeito de bot
}
