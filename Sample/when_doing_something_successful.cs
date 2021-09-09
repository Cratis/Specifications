using Aksio.Specifications;
using Xunit;

namespace Sample
{
    public class when_doing_something_successful
    {
        void Establish() { }

        void Because() { }

        [Fact] void should_be_true() => true.ShouldBeTrue();
    }
}