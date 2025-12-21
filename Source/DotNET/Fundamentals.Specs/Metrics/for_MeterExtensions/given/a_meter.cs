// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Metrics;

namespace Cratis.Metrics.for_MeterExtensions.given;

public class a_meter : Specification
{
    protected IMeter<a_meter> _meter;
    protected Meter _actualMeter;

    void Establish()
    {
        _actualMeter = new Meter("TestMeter");
        _meter = new Mock<IMeter<a_meter>>().Object;
        Mock.Get(_meter).Setup(_ => _.ActualMeter).Returns(_actualMeter);
    }
}
