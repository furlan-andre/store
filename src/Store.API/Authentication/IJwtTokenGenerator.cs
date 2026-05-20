namespace Store.API.Authentication;

public interface IJwtTokenGenerator
{
    AuthTokenResponse Generate(string username);
}
