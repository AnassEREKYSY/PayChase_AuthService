using System.IdentityModel.Tokens.Jwt;
using Core.Dtos;
using Core.Responses;
using Infrastructure.IRepositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService _svc, IUserRepository _users) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MeResponse>> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        try
        {
            var me = await _svc.RegisterAsync(req, ct);
            return Ok(me);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                status = 400,
                code = "auth.email_taken",
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        try
        {
            var tokens = await _svc.LoginAsync(req, ct);
            return Ok(tokens);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new
            {
                status = 401,
                code = "auth.invalid_credentials",
                message = ex.Message
            });
        }
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MeResponse>> Me(CancellationToken ct)
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(sub)) return Unauthorized();
        var u = await _users.GetByIdAsync(sub, ct);
        if (u is null) return NotFound();

        return Ok(new MeResponse(
            u.Id, u.OrgId, u.Email, u.Name,
            u.Roles.ToArray(), u.Status.ToString(), u.CreatedAt));
    }
}
