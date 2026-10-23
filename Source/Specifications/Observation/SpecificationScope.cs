// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace Cratis.Specifications;

/// <summary>
/// Represents one observed specification instance, from before Establish until after Destroy.
/// </summary>
/// <remarks>
/// An adapter creates a scope only when <see cref="SpecificationObservers.Any"/> is true at the start of the
/// lifecycle, and then runs Establish, Because and Destroy through it. The observers registered when the scope was
/// created are the ones it reports to for its whole life, so every observer sees a complete sequence.
/// The scope is <see cref="Current"/> while its observers start it and while its lifecycle methods run.
/// </remarks>
/// <param name="instance">The specification instance.</param>
/// <param name="displayName">The display name, or null to derive it from the specification type.</param>
internal sealed class SpecificationScope(object instance, string? displayName = null)
{
    static readonly AsyncLocal<SpecificationScope?> _current = new();

    readonly ISpecificationObserver[] _observers = SpecificationObservers.All;
    List<IAsyncDisposable>? _ownedLeases;
    int _ended;

    /// <summary>
    /// Gets the scope whose observers or lifecycle methods are running, if any.
    /// </summary>
    public static SpecificationScope? Current => _current.Value;

    /// <summary>
    /// Gets the specification instance.
    /// </summary>
    public object Instance { get; } = instance;

    /// <summary>
    /// Gets the type of the specification instance.
    /// </summary>
    public Type SpecificationType { get; } = instance.GetType();

    /// <summary>
    /// Gets the display name of the specification.
    /// </summary>
    public string DisplayName { get; } = displayName ?? SpecificationDisplayNames.ForType(instance.GetType());

    /// <summary>
    /// Gets the leases owned by the scope, for observers that release them when the scope ends.
    /// </summary>
    public IList<IAsyncDisposable> OwnedLeases => _ownedLeases ??= [];

    /// <summary>
    /// Creates a scope for a specification instance when any observer is registered.
    /// </summary>
    /// <param name="instance">The specification instance.</param>
    /// <param name="displayName">The display name, or null to derive it from the specification type.</param>
    /// <returns>The scope, or null when nothing observes the lifecycle.</returns>
    public static SpecificationScope? CreateIfObserved(object instance, string? displayName = null) =>
        SpecificationObservers.Any ? new SpecificationScope(instance, displayName) : null;

    /// <summary>
    /// Tells the observers the scope is starting.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask Start()
    {
        _current.Value = this;
        foreach (var observer in _observers)
        {
            await observer.ScopeStarting(this);
        }
    }

    /// <summary>
    /// Runs the Establish lifecycle methods and reports the phase.
    /// </summary>
    /// <param name="specificationBase">The framework base type at which the inheritance walk stops.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Establish(Type specificationBase) => RunLifecycle(SpecificationPhase.Establish, specificationBase);

    /// <summary>
    /// Runs the Because lifecycle methods and reports the phase.
    /// </summary>
    /// <param name="specificationBase">The framework base type at which the inheritance walk stops.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Because(Type specificationBase) => RunLifecycle(SpecificationPhase.Because, specificationBase);

    /// <summary>
    /// Runs the Destroy lifecycle methods and reports the phase.
    /// </summary>
    /// <param name="specificationBase">The framework base type at which the inheritance walk stops.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task Destroy(Type specificationBase) => RunLifecycle(SpecificationPhase.Destroy, specificationBase);

    /// <summary>
    /// Runs a phase body and reports the phase.
    /// </summary>
    /// <param name="phase">The phase.</param>
    /// <param name="body">The body of the phase, or null when the specification has nothing to run for it.</param>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask Run(SpecificationPhase phase, Func<object, ValueTask>? body)
    {
        _current.Value = this;
        var start = Stopwatch.GetTimestamp();
        try
        {
            if (body is not null) await body(Instance);
        }
        catch (Exception ex)
        {
            Completed(phase, start, ex);
            throw;
        }

        Completed(phase, start, null);
    }

    /// <summary>
    /// Tells the observers the scope has ended. Only the first call has an effect.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
    public async ValueTask End()
    {
        if (Interlocked.Exchange(ref _ended, 1) == 1) return;

        foreach (var observer in _observers)
        {
            await observer.ScopeEnded(this);
        }
    }

    async Task RunLifecycle(SpecificationPhase phase, Type specificationBase)
    {
        _current.Value = this;
        var start = Stopwatch.GetTimestamp();
        try
        {
            await (phase switch
            {
                SpecificationPhase.Establish => SpecificationLifecycle.Establish(Instance, SpecificationType, specificationBase),
                SpecificationPhase.Because => SpecificationLifecycle.Because(Instance, SpecificationType, specificationBase),
                _ => SpecificationLifecycle.Destroy(Instance, SpecificationType, specificationBase)
            });
        }
        catch (Exception ex)
        {
            Completed(phase, start, ex);
            throw;
        }

        Completed(phase, start, null);
    }

    void Completed(SpecificationPhase phase, long start, Exception? failure)
    {
        var end = Stopwatch.GetTimestamp();
        foreach (var observer in _observers)
        {
            observer.PhaseCompleted(this, phase, start, end, failure);
        }
    }
}
