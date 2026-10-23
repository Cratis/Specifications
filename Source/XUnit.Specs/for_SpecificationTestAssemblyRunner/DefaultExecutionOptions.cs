// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit.Abstractions;

namespace Cratis.Specifications.for_SpecificationTestAssemblyRunner;

/// <summary>
/// Execution options with no value set, so the runner uses its defaults.
/// </summary>
public class DefaultExecutionOptions : Xunit.LongLivedMarshalByRefObject, ITestFrameworkExecutionOptions
{
    public TValue GetValue<TValue>(string name) => default;

    public void SetValue<TValue>(string name, TValue value)
    {
    }
}
