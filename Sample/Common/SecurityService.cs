namespace Sample;

public class SecurityService
{
    public UserToken Authenticate(string username, string password)
    {
        if (username == null || password == null) throw new UserMustBeSpecified();
        return new UserToken
        {
            Role = Roles.Admin,
            SessionId = Guid.NewGuid().ToString()
        };
    }

    public Task<UserToken> AuthenticateAsync(string username, string password)
    {
        if (username == null || password == null) throw new UserMustBeSpecified();
        return Task.FromResult(new UserToken
        {
            Role = Roles.Admin,
            SessionId = Guid.NewGuid().ToString()
        });
    }
}
