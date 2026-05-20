using Microsoft.Extensions.Options;

namespace Store.API.Authentication;

public sealed class AuthService(
    IOptions<AuthCredentialsOptions> credentialsOptions,
    IJwtTokenGenerator jwtTokenGenerator) : IAuthService
{
    private readonly AuthCredentialsOptions _credentials = credentialsOptions.Value;

    public AuthTokenResponse? Authenticate(LoginRequest request)
    {
        if (!string.Equals(request.Username, _credentials.Username, StringComparison.Ordinal) ||
            !string.Equals(request.Password, _credentials.Password, StringComparison.Ordinal))
        {
            return null;
        }

        return jwtTokenGenerator.Generate(request.Username);
    }
}
