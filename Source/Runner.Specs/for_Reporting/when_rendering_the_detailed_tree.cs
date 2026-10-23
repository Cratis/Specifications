// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Specifications.Runner.for_Reporting;

public class when_rendering_the_detailed_tree : Specification
{
    string result;

    void Because() => result = rendered_tree.Render(new(OutputLevel.Detailed, false, false, false, null, []));

    void should_match_the_snapshot() => result.ShouldEqual(approved_snapshot.Approved("detailed.txt", result));
}
