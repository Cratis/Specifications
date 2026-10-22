// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Sample.for_SecurityServiceAsync.given;

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
