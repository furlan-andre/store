namespace Store.API.Authentication;

public interface IAuthService
{
    AuthTokenResponse? Authenticate(LoginRequest request);
}
