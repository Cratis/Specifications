// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Runtime.CompilerServices;
using LifecycleProbes;

namespace Cratis.Specifications.Runner.for_LifecycleContract;

public class when_cancelling_lifecycle_methods : Specification
{
    readonly List<(Exception Generated, Exception Reflected)> results = [];

    async Task Because()
    {
        await Compare<setup_cancellation>("Establish");
        await Compare<async_setup_cancellation>("Establish");
        await Compare<act_cancellation>("Because");
        await Compare<async_act_cancellation>("Because");
        await Compare<cleanup_cancellation>("Destroy");
        await Compare<async_cleanup_cancellation>("Destroy");
    }

    async Task Compare<T>(string phase)
        where T : lifecycle_context, new()
    {
        RuntimeHelpers.RunModuleConstructor(typeof(T).Assembly.ManifestModule.ModuleHandle);
        var descriptor = given.contract_probes.WorkFor<T>().Specification;
        var generated = phase switch
        {
            "Establish" => descriptor.Establish,
            "Because" => descriptor.Because,
            _ => descriptor.Destroy
        };
        results.Add((await Capture(() => generated(new T()).AsTask()), await Capture(() => Reflected<T>(phase))));
    }

    static Task Reflected<T>(string phase)
        where T : lifecycle_context, new() => phase switch
        {
            "Establish" => SpecificationLifecycle.Establish(new T(), typeof(T), typeof(Specification)),
            "Because" => SpecificationLifecycle.Because(new T(), typeof(T), typeof(Specification)),
            _ => SpecificationLifecycle.Destroy(new T(), typeof(T), typeof(Specification))
        };

    static async Task<Exception> Capture(Func<Task> action)
    {
        try
        {
            await action();
            throw new InvalidOperationException("The lifecycle cancellation did not throw.");
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    void should_reproduce_all_six_sync_and_async_cases() => results.Count.ShouldEqual(6);

    void should_match_synchronous_reflection_cancellation()
    {
        foreach (var result in results.Where((_, index) => index % 2 == 0)) result.Generated.GetType().ShouldEqual(result.Reflected.GetType());
    }

    void should_match_asynchronous_reflection_cancellation()
    {
        foreach (var result in results.Where((_, index) => index % 2 != 0)) result.Generated.GetType().ShouldEqual(result.Reflected.GetType());
    }

    void should_preserve_the_phase_and_existing_message()
    {
        foreach (var result in results) result.Generated.Message.ShouldEqual(result.Reflected.Message);
    }

    void should_preserve_the_original_cancellation()
    {
        foreach (var result in results)
        {
            result.Reflected.ShouldBeOfExactType<SpecificationCancelledException>();
            result.Generated.InnerException.ShouldNotBeNull();
            result.Generated.InnerException.GetType().ShouldEqual(result.Reflected.InnerException.GetType());
            result.Generated.InnerException?.Message.ShouldEqual(result.Reflected.InnerException?.Message);
        }
    }
}
