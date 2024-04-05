namespace Sample;

public class when_doing_something_successful
{
    void Establish() { }

    void Because() { }

    [Test] public void should_be_true() => true.ShouldBeTrue();
}
