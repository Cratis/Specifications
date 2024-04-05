namespace Sample.for_SecurityServiceAsync;

public class When_authenticating_an_admin_user : given.no_user_authenticated
{
    UserToken user_token;

    Task Establish()
    {
        Console.WriteLine("Establish in specification");
        return Task.Delay(50);
    }

    void Because() => user_token = subject.Authenticate("username", "password");

    [Test] public void should_indicate_the_users_role() => user_token.Role.ShouldEqual(Roles.Admin);

    [Test] public void should_have_a_unique_session_id() => user_token.SessionId.ShouldNotBeNull();
}
