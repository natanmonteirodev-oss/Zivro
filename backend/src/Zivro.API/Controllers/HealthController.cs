namespace Zivro.API.Controllers;

using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller para verificar o status de saúde da API.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Verifica o status de saúde da API.
    /// </summary>
    /// <returns>Status da API, timestamp e ambiente.</returns>
    /// <response code="200">API está saudável.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }
}
