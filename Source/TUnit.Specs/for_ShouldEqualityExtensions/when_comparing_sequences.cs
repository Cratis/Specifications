// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections;

namespace Cratis.Specifications.for_ShouldEqualityExtensions;

public class when_comparing_sequences : Specification
{
    sealed class DisposableSequence(int[] items) : IEnumerable
    {
        public int Disposed { get; private set; }

        public IEnumerator GetEnumerator() => new Enumerator(this, items);

        sealed class Enumerator(DisposableSequence owner, int[] items) : IEnumerator, IDisposable
        {
            int _index = -1;

            public object Current => items[_index];

            public bool MoveNext() => ++_index < items.Length;

            public void Reset() => _index = -1;

            public void Dispose() => owner.Disposed++;
        }
    }

    readonly DisposableSequence _expected = new([1, 2]);
    readonly DisposableSequence _actual = new([1, 2]);
    Exception _result;

    void Because() => _result = Catch.Exception(() => ((object)_actual).ShouldEqual(_expected));

    [Test] public void should_consider_them_equal() => _result.ShouldBeNull();

    [Test] public void should_dispose_the_expected_enumerator() => _expected.Disposed.ShouldEqual(1);

    [Test] public void should_dispose_the_actual_enumerator() => _actual.Disposed.ShouldEqual(1);
}
