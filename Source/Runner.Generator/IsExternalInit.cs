// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// netstandard2.0 does not define IsExternalInit, which the compiler needs for records and init accessors.
namespace System.Runtime.CompilerServices;

/// <summary>
/// Reserved for use by the compiler for tracking metadata on init-only setters.
/// </summary>
internal static class IsExternalInit
{
}
