using Aksio.Specifications;
using Xunit;

namespace Sample.for_Authentication
{
    public class When_authenticating_an_admin_user : Specification
    {
        SecurityService subject;
        UserToken user_token;

        void Establish() => subject = new SecurityService();

        void Because() => user_token = subject.Authenticate("username", "password");

        [Fact]
        void should_indicate_the_users_role() => user_token.Role.ShouldEqual(Roles.Admin);

        [Fact]
        void should_have_a_unique_session_id() => user_token.SessionId.ShouldNotBeNull();
    }

}