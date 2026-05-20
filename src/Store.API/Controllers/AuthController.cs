using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.API.Authentication;

namespace Store.API.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("token")]
    public IActionResult Login(LoginRequest request)
    {
        var token = authService.Authenticate(request);

        return token is null ? Unauthorized() : Ok(token);
    }
}
