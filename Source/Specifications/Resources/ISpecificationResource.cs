// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace Cratis.Specifications.Resources;

/// <summary>
/// Defines an expensive resource that specifications share for the whole test session, such as a database server,
/// a container or a compiled set of inputs.
/// </summary>
/// <remarks>
/// <para>
/// A resource is created and started the first time a specification asks for it through
/// <see cref="SpecificationResources"/>, never before, so a test run that does not ask for it never starts it.
/// It is started once per test session, however many specifications ask for it at the same time, and stopped through
/// <see cref="IAsyncDisposable.DisposeAsync"/> when the session ends.
/// </para>
/// <para>
/// When <see cref="Start"/> throws, the resource is disposed straight away and every specification that asks for it
/// fails with that same exception; it is not started a second time.
/// </para>
/// </remarks>
[Experimental(SpecificationResources.DiagnosticId)]
public interface ISpecificationResource : IAsyncDisposable
{
    /// <summary>
    /// Starts the resource.
    /// </summary>
    /// <param name="cancellationToken">Cancelled when the test session ends before the resource has started.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    ValueTask Start(CancellationToken cancellationToken);
}
