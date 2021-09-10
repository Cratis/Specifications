using Aksio.Specifications;

namespace Sample.for_SecurityServiceAsync.given
{
    public class no_user_authenticated : Specification
    {
        protected SecurityService subject;

        Task Establish()
        {
            Console.WriteLine("Establish in given statement");
            subject = new SecurityService();
            return Task.CompletedTask;
        }
    }
}