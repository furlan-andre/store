namespace Store.API.Authentication;

public sealed class AuthTokenResponse
{
    public required string AccessToken { get; init; }

    public required DateTimeOffset ExpiresAt { get; init; }
}
