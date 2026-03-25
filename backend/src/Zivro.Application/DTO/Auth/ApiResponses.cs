namespace Zivro.Application.DTO.Auth
{
    /// <summary>
    /// Request para renovar token de acesso usando refresh token.
    /// </summary>
    public class RefreshTokenRequest
    {
        /// <summary>
        /// Token de refresh obtido no login/registro.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Resposta genérica para erros de API.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Mensagem de erro legível.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Código de erro para tratamento em cliente.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp do erro (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Resposta genérica para operações bem-sucedidas.
    /// </summary>
    public class SuccessResponse
    {
        /// <summary>
        /// Mensagem de sucesso.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Código de sucesso para tratamento em cliente.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp da operação (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Resposta para validação de credenciais.
    /// </summary>
    public class ValidateResponse
    {
        /// <summary>
        /// Indica se as credenciais são válidas.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Mensagem descritiva.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp da validação (UTC).
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
