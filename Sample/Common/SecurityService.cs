// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
