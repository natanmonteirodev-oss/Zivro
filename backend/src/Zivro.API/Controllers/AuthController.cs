namespace Zivro.API.Controllers;

using Zivro.Application.DTO.Auth;
using Zivro.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

/// <summary>
/// Controller for authentication and authorization operations.
/// Implements secure registration, login, token refresh, and 2FA.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailVerificationService _emailVerificationService;
    private readonly ITwoFactorAuthService _twoFactorAuthService;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes a new instance of AuthController.
    /// </summary>
    public AuthController(
        IAuthService authService,
        IAuditLogService auditLogService,
        IEmailVerificationService emailVerificationService,
        ITwoFactorAuthService twoFactorAuthService,
        ILogger<AuthController> logger)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _emailVerificationService = emailVerificationService ?? throw new ArgumentNullException(nameof(emailVerificationService));
        _twoFactorAuthService = twoFactorAuthService ?? throw new ArgumentNullException(nameof(twoFactorAuthService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a new user account with email and password.
    /// Email verification will be sent to the provided email address.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [EnableRateLimiting("auth-register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (request == null)
            {
                _logger.LogWarning("Register: request body is null");
                return BadRequest(new ErrorResponse { Message = "RequisiÃ§Ã£o invÃ¡lida.", Code = "INVALID_REQUEST" });
            }

            var response = await _authService.RegisterAsync(request);

            // Log successful registration
            await _auditLogService.LogActionAsync(response.UserId, request.Email, "Register", true, ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            // Send verification email
            await _emailVerificationService.SendVerificationEmailAsync(response.UserId, request.Email);

            _logger.LogInformation("User registered and verification email sent. Email: {Email}", request.Email);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Register validation error: {Message}", ex.Message);
            await _auditLogService.LogActionAsync(null, request?.Email ?? "unknown", "Register", false, failureReason: ex.Message, ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());
            return BadRequest(new ErrorResponse { Message = ex.Message, Code = "VALIDATION_ERROR" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Register error");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Erro ao registrar usuÃ¡rio.", Code = "REGISTRATION_ERROR" });
        }
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// Returns JWT access token and refresh token for subsequent API calls.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("auth-login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
            {
                _logger.LogWarning("Login: invalid request");
                return BadRequest(new ErrorResponse { Message = "Email e senha sÃ£o obrigatÃ³rios.", Code = "INVALID_REQUEST" });
            }

            // Check rate limiting on failed attempts
            var failedAttempts = await _auditLogService.GetFailedLoginAttemptsAsync(request.Email, minutesBack: 60);
            if (failedAttempts >= 10)
            {
                _logger.LogWarning("Login: too many failed attempts for email {Email}", request.Email);
                await _auditLogService.LogActionAsync(null, request.Email, "FailedLogin", false, "Muitas tentativas falhadas", ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());
                return StatusCode(StatusCodes.Status429TooManyRequests, new ErrorResponse { Message = "Muitas tentativas falhadas. Tente novamente mais tarde.", Code = "TOO_MANY_ATTEMPTS" });
            }

            var response = await _authService.LoginAsync(request);

            // Log successful login
            await _auditLogService.LogActionAsync(response.UserId, request.Email, "Login", true, ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            _logger.LogInformation("User logged in successfully. Email: {Email}", request.Email);

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login unauthorized: {Message}", ex.Message);
            await _auditLogService.LogActionAsync(null, request?.Email ?? "unknown", "FailedLogin", false, failureReason: ex.Message, ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());
            return Unauthorized(new ErrorResponse { Message = ex.Message, Code = "UNAUTHORIZED" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login error");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Erro ao realizar login.", Code = "LOGIN_ERROR" });
        }
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Returns new access token and refresh token pair.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new ErrorResponse { Message = "Refresh token Ã© obrigatÃ³rio.", Code = "INVALID_REQUEST" });

            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new ErrorResponse { Message = ex.Message, Code = "UNAUTHORIZED" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refresh token error");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Erro ao renovar token.", Code = "REFRESH_ERROR" });
        }
    }

    /// <summary>
    /// Logs out the current user by revoking their refresh token.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest(new ErrorResponse { Message = "Refresh token Ã© obrigatÃ³rio.", Code = "INVALID_REQUEST" });

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "unknown";

            await _authService.RevokeTokenAsync(request.RefreshToken);

            if (Guid.TryParse(userId, out var guidUserId))
                await _auditLogService.LogActionAsync(guidUserId, email, "Logout", true, ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString());

            _logger.LogInformation("User logged out. Email: {Email}", email);

            return Ok(new SuccessResponse { Message = "Logout realizado com sucesso.", Code = "LOGOUT_SUCCESS" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout error");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Erro ao realizar logout.", Code = "LOGOUT_ERROR" });
        }
    }

    /// <summary>
    /// Validates user credentials without issuing tokens.
    /// Used for additional verification steps or authentication checks.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ValidateResponse>> Validate([FromBody] LoginRequest request)
    {
        try
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new ErrorResponse { Message = "Email e senha sÃ£o obrigatÃ³rios.", Code = "INVALID_REQUEST" });

            var isValid = await _authService.ValidateUserAsync(request.Email, request.Password);

            return Ok(new ValidateResponse { IsValid = isValid, Message = isValid ? "Credenciais vÃ¡lidas." : "Credenciais invÃ¡lidas." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validate error");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Erro ao validar credenciais.", Code = "VALIDATION_ERROR" });
        }
    }

    /// <summary>
    /// Verifies user email address using verification token.
    /// Must be called after user receives verification email.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("verify-email")]
    [EnableRateLimiting("email-verify")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResponse>> VerifyEmail([FromQuery] Guid userId, [FromQuery] string token)
    {
        try
        {
            if (userId == Guid.Empty || string.IsNullOrWhiteSpace(token))
                return BadRequest(new ErrorResponse { Message = "userId e token sÃ£o obrigatÃ³rios.", Code = "INVALID_REQUEST" });

            var isVerified = await _emailVerificationService.VerifyEmailAsync(userId, token);

            if (!isVerified)
            {
                _logger.LogWarning("Email verification failed for user {UserId}", userId);
                return Unauthorized(new ErrorResponse { Message = "Token de verificaÃ§Ã£o invÃ¡lido ou expirado.", Code = "INVALID_TOKEN" });
            }

            _logger.LogInformation("Email verified for user {UserId}", userId);

            return Ok(new SuccessResponse { Message = "Email verificado com sucesso!", Code = "EMAIL_VERIFIED" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email verification error");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Erro ao verificar email.", Code = "VERIFICATION_ERROR" });
        }
    }

    /// <summary>
    /// Initiates Two-Factor Authentication (2FA) setup for authenticated user.
    /// Returns QR code and backup codes for account recovery.
    /// </summary>
    [Authorize]
    [HttpPost("2fa/setup")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Setup2FA()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "unknown";

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new ErrorResponse { Message = "UsuÃ¡rio nÃ£o identificado.", Code = "INVALID_USER" });

            var (secretKey, qrCode, backupCodes) = await _twoFactorAuthService.SetupTwoFactorAsync(userId, email);

            _logger.LogInformation("2FA setup initiated for user {UserId}", userId);

            return Ok(new
            {
                secretKey,
                qrCode,
                backupCodes,
                message = "Escaneie o cÃ³digo QR com seu aplicativo autenticador (Google Authenticator, Microsoft Authenticator, etc.) e confirme com um cÃ³digo TOTP."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "2FA setup error");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Erro ao configurar 2FA.", Code = "2FA_SETUP_ERROR" });
        }
    }

    /// <summary>
    /// Confirms Two-Factor Authentication setup by validating TOTP code.
    /// 2FA must be confirmed before it becomes active.
    /// </summary>
    [Authorize]
    [HttpPost("2fa/confirm")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResponse>> Confirm2FA([FromBody] dynamic request)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new ErrorResponse { Message = "UsuÃ¡rio nÃ£o identificado.", Code = "INVALID_USER" });

            var totpCode = request?.totpCode as string;
            if (string.IsNullOrWhiteSpace(totpCode))
                return BadRequest(new ErrorResponse { Message = "CÃ³digo TOTP Ã© obrigatÃ³rio.", Code = "INVALID_REQUEST" });

            var isConfirmed = await _twoFactorAuthService.ConfirmTwoFactorAsync(userId, totpCode);

            if (!isConfirmed)
                return BadRequest(new ErrorResponse { Message = "CÃ³digo TOTP invÃ¡lido.", Code = "INVALID_TOTP" });

            _logger.LogInformation("2FA enabled for user {UserId}", userId);

            return Ok(new SuccessResponse { Message = "AutenticaÃ§Ã£o de dois fatores ativada com sucesso!", Code = "2FA_ENABLED" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "2FA confirm error");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Erro ao confirmar 2FA.", Code = "2FA_CONFIRM_ERROR" });
        }
    }

    /// <summary>
    /// Disables Two-Factor Authentication for authenticated user.
    /// </summary>
    [Authorize]
    [HttpPost("2fa/disable")]
    [ProducesResponseType(typeof(SuccessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SuccessResponse>> Disable2FA()
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new ErrorResponse { Message = "UsuÃ¡rio nÃ£o identificado.", Code = "INVALID_USER" });

            await _twoFactorAuthService.DisableTwoFactorAsync(userId);

            _logger.LogInformation("2FA disabled for user {UserId}", userId);

            return Ok(new SuccessResponse { Message = "AutenticaÃ§Ã£o de dois fatores desativada.", Code = "2FA_DISABLED" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "2FA disable error");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Erro ao desativar 2FA.", Code = "2FA_DISABLE_ERROR" });
        }
    }

    /// <summary>
    /// Gets audit logs for authenticated user.
    /// Shows login/logout history and security events.
    /// </summary>
    [Authorize]
    [HttpGet("audit-logs")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> GetAuditLogs([FromQuery] int limit = 50)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new ErrorResponse { Message = "UsuÃ¡rio nÃ£o identificado.", Code = "INVALID_USER" });

            var logs = await _auditLogService.GetUserAuditLogsAsync(userId, limit);

            return Ok(new { logs, message = "HistÃ³rico de atividades do usuÃ¡rio" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Audit logs error");
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse { Message = "Erro ao recuperar histÃ³rico.", Code = "AUDIT_LOGS_ERROR" });
        }
    }
}
