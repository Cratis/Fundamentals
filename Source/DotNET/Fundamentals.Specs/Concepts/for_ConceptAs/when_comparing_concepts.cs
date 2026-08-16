// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Concepts.given;

namespace Cratis.Concepts.for_ConceptAs;

public class when_comparing_concepts : Specification
{
    [Fact]
    public void should_consider_equal_concepts_equal()
    {
        var conceptA = new IntConcept(42);
        var conceptB = new IntConcept(42);

        conceptA.ShouldEqual(conceptB);
        conceptA.Equals(conceptB).ShouldBeTrue();
    }

    [Fact]
    public void should_consider_unequal_concepts_unequal()
    {
        var conceptA = new IntConcept(42);
        var conceptB = new IntConcept(24);

        conceptA.ShouldNotEqual(conceptB);
        conceptA.Equals(conceptB).ShouldBeFalse();
    }

    [Fact]
    public void should_order_greater_concept_higher()
    {
        var conceptA = new IntConcept(42);
        var conceptB = new IntConcept(24);

        Assert.True(conceptA.CompareTo(conceptB) > 0);
        conceptA.ShouldBeGreaterThan(conceptB);
    }

    [Fact]
    public void should_order_lesser_concept_lower()
    {
        var conceptA = new IntConcept(24);
        var conceptB = new IntConcept(42);

        Assert.True(conceptA.CompareTo(conceptB) < 0);
        conceptA.ShouldBeLessThan(conceptB);
    }

    [Fact]
    public void should_give_equal_concepts_same_hash_code()
    {
        var conceptA = new IntConcept(42);
        var conceptB = new IntConcept(42);

        conceptA.GetHashCode().ShouldEqual(conceptB.GetHashCode());
    }

    [Fact]
    public void should_give_unequal_concepts_different_hash_codes()
    {
        var conceptA = new IntConcept(42);
        var conceptB = new IntConcept(24);

        conceptA.GetHashCode().ShouldNotEqual(conceptB.GetHashCode());
    }
}
