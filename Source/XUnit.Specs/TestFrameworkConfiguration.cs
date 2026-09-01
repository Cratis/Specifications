// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Opt in to the shared specification lifecycle - the feature under test.
[assembly: Xunit.TestFramework("Cratis.Specifications.SpecificationTestFramework", "Cratis.Specifications.XUnit")]

// Run collections sequentially and in a deterministic order so a follow-up spec class can observe
// what the shared lifecycle did to an earlier class (such as Destroy running once after its last fact).
[assembly: Xunit.CollectionBehavior(DisableTestParallelization = true)]
[assembly: Xunit.TestCollectionOrderer("Cratis.Specifications.Support.AlphabeticalTestCollectionOrderer", "Cratis.Specifications.XUnit.Specs")]
