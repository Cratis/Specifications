namespace Sample.for_SecurityService;

public class When_authenticating_an_admin_user : given.no_user_authenticated
{
    UserToken user_token;

    void Because() => user_token = subject.Authenticate("username", "password");

    [Test] public void should_indicate_the_users_role() => user_token.Role.ShouldEqual(Roles.Admin);

    [Test] public void should_have_a_unique_session_id() => user_token.SessionId.ShouldNotBeNull();
}
