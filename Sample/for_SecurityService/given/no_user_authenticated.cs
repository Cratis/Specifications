using Aksio.Specifications;

namespace Sample.for_Authentication.given
{
    public class no_user_authenticated : Specification
    {
        protected SecurityService subject;

        void Establish() => subject = new SecurityService();
    }
}