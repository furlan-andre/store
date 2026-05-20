namespace Store.API.Authentication;

public sealed class AuthCredentialsOptions
{
    public const string SectionName = "Authentication";

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
