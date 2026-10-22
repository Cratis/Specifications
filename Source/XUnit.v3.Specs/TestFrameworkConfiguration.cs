// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Opt in to the shared specification lifecycle - the feature under test.
[assembly: Xunit.TestFramework(typeof(Cratis.Specifications.SpecificationTestFramework))]

// Run collections sequentially and in a deterministic order so a follow-up spec class can observe
// what the shared lifecycle did to an earlier class (such as Destroy running once after its last fact).
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]
[assembly: Xunit.TestCollectionOrderer(typeof(Cratis.Specifications.Support.AlphabeticalTestCollectionOrderer))]
