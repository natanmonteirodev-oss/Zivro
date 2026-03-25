namespace Zivro.Application.Services;

using Zivro.Application.DTO.Auth;
using Zivro.Application.Interfaces;
using Zivro.Domain.Entities;
using Zivro.Domain.Interfaces;

/// <summary>
/// Service for handling authentication operations.
/// Implements business logic for registration, login, and token refresh.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    // Token configuration constants
    private const int AccessTokenExpirationMinutes = 15;
    private const int RefreshTokenExpirationDays = 7;

    /// <summary>
    /// Initializes a new instance of the AuthService.
    /// </summary>
    /// <param name="authRepository">Repository for authentication data access.</param>
    /// <param name="passwordHasher">Service for password hashing and verification.</param>
    /// <param name="jwtTokenService">Service for JWT token generation and validation.</param>
    public AuthService(
        IAuthRepository authRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
    }

    /// <summary>
    /// Registers a new user with email and password.
    /// </summary>
    /// <param name="request">The registration request containing user details.</param>
    /// <returns>Authentication response with access and refresh tokens.</returns>
    /// <exception cref="InvalidOperationException">If email already exists or validation fails.</exception>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate request
        ValidateRegisterRequest(request);

        // Check if email already exists
        var existingUser = await _authRepository.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Email já está registrado.");

        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = request.Email.ToLowerInvariant().Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Save user
        await _authRepository.CreateUserAsync(user);
        await _authRepository.SaveChangesAsync();

        // Generate tokens
        return await GenerateAuthResponseAsync(user);
    }

    /// <summary>
    /// Authenticates a user with email and password.
    /// </summary>
    /// <param name="request">The login request containing credentials.</param>
    /// <returns>Authentication response with tokens if successful.</returns>
    /// <exception cref="UnauthorizedAccessException">If credentials are invalid.</exception>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            throw new UnauthorizedAccessException("Email e senha são obrigatórios.");

        // Get user
        var user = await _authRepository.GetUserByEmailAsync(request.Email.ToLowerInvariant().Trim());
        if (user == null)
            throw new UnauthorizedAccessException("Email ou senha inválidos.");

        // Verify password
        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Email ou senha inválidos.");

        // Check if user is active
        if (!user.IsActive)
            throw new UnauthorizedAccessException("Usuário desativado.");

        // Generate tokens
        return await GenerateAuthResponseAsync(user);
    }

    /// <summary>
    /// Refreshes an expired access token using a refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token value.</param>
    /// <returns>New authentication response with fresh tokens.</returns>
    /// <exception cref="UnauthorizedAccessException">If refresh token is invalid or expired.</exception>
    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new UnauthorizedAccessException("Refresh token é obrigatório.");

        // Get refresh token
        var storedToken = await _authRepository.GetRefreshTokenAsync(refreshToken);
        if (storedToken == null)
            throw new UnauthorizedAccessException("Refresh token inválido.");

        // Validate token
        if (!storedToken.IsValid())
            throw new UnauthorizedAccessException("Refresh token expirado ou revogado.");

        // Get user
        var user = await _authRepository.GetUserByIdAsync(storedToken.UserId);
        if (user == null || !user.IsActive)
            throw new UnauthorizedAccessException("Usuário não encontrado ou desativado.");

        // Revoke old refresh token
        await _authRepository.RevokeRefreshTokenAsync(storedToken);

        // Generate new tokens
        return await GenerateAuthResponseAsync(user);
    }

    /// <summary>
    /// Revokes a refresh token (logout).
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    public async Task RevokeTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return;

        var token = await _authRepository.GetRefreshTokenAsync(refreshToken);
        if (token != null)
        {
            await _authRepository.RevokeRefreshTokenAsync(token);
        }
    }

    /// <summary>
    /// Validates user credentials without issuing tokens.
    /// </summary>
    /// <param name="email">The user's email.</param>
    /// <param name="password">The user's password.</param>
    /// <returns>True if credentials are valid and user is active.</returns>
    public async Task<bool> ValidateUserAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return false;

        var user = await _authRepository.GetUserByEmailAsync(email.ToLowerInvariant().Trim());
        if (user == null || !user.IsActive)
            return false;

        return _passwordHasher.Verify(password, user.PasswordHash);
    }

    /// <summary>
    /// Generates an authentication response with access and refresh tokens.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    /// <returns>Authentication response with tokens.</returns>
    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        // Generate JWT access token
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email, user.Name);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(AccessTokenExpirationMinutes);

        // Generate refresh token
        var refreshTokenValue = GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(RefreshTokenExpirationDays);

        // Save refresh token
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = refreshTokenExpiresAt,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _authRepository.CreateRefreshTokenAsync(refreshToken);
        await _authRepository.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            TokenType = "Bearer"
        };
    }

    /// <summary>
    /// Generates a secure random refresh token.
    /// </summary>
    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    /// <summary>
    /// Validates the registration request.
    /// </summary>
    private static void ValidateRegisterRequest(RegisterRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new InvalidOperationException("Nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new InvalidOperationException("Email é obrigatório.");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new InvalidOperationException("Senha é obrigatória.");

        if (request.Password.Length < 6)
            throw new InvalidOperationException("Senha deve ter no mínimo 6 caracteres.");

        if (request.Password != request.PasswordConfirmation)
            throw new InvalidOperationException("Senhas não correspondem.");

        // Basic email validation
        if (!IsValidEmail(request.Email))
            throw new InvalidOperationException("Email inválido.");
    }

    /// <summary>
    /// Basic email validation.
    /// </summary>
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
