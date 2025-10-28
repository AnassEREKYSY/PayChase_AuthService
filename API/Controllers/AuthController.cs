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
    [HttpPost("register")]
    public async Task<ActionResult<MeResponse>> Register(RegisterRequest req, CancellationToken ct) =>
        Ok(await _svc.RegisterAsync(req, ct));

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest req, CancellationToken ct) =>
        Ok(await _svc.LoginAsync(req, ct));

    [Authorize]
    [HttpGet("me")]
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
