using Aksio.Specifications;
using Xunit;

namespace Sample.for_Authentication
{
    public class When_authenticating_a_null_user : given.no_user_authenticated
    {
        Exception result;

        void Because() => result = Catch.Exception(() => subject.Authenticate(null, null));

        [Fact] void should_throw_user_must_be_specified_exception() => result.ShouldBeOfExactType<UserMustBeSpecified>();
    }
}