// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner;

/// <summary>
/// The exception that fails every fact of a specification class that could not be discovered correctly.
/// </summary>
/// <param name="message">The discovery error.</param>
public sealed class SpecificationDiscoveryException(string message) : Exception(message);
