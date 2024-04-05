namespace Sample.for_SecurityServiceAsync;

public class When_authenticating_a_null_user : given.no_user_authenticated
{
    Exception result;

    Task Establish()
    {
        Console.WriteLine("Establish in specification");
        return Task.Delay(50);
    }

    Task Because()
    {
        result = Catch.Exception(() => subject.Authenticate(null, null));
        return Task.Delay(50);
    }

    [Test] public void should_throw_user_must_be_specified_exception() => result.ShouldBeOfExactType<UserMustBeSpecified>();
}
