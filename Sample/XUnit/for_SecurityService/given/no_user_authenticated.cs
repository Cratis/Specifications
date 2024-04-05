namespace Sample.for_SecurityService.given;

public class no_user_authenticated : Specification
{
    protected SecurityService subject;

    void Establish() => subject = new SecurityService();
}
