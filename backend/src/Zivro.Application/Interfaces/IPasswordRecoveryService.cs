namespace Zivro.Application.Interfaces;

/// <summary>
/// Serviço para gerenciar recuperação de senha
/// </summary>
public interface IPasswordRecoveryService
{
    /// <summary>
    /// Solicita reset de senha (envia email com link)
    /// </summary>
    Task RequestPasswordResetAsync(string email);

    /// <summary>
    /// Valida se o token de reset é válido
    /// </summary>
    Task<bool> ValidateResetTokenAsync(string token);

    /// <summary>
    /// Reseta a senha usando o token
    /// </summary>
    Task<bool> ResetPasswordAsync(string token, string newPassword);

    /// <summary>
    /// Verifica quantas solicitações de reset um email fez
    /// </summary>
    Task<int> GetResetRequestCountAsync(string email, int minutesBack = 60);
}
