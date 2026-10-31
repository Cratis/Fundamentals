// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// The type discovery event source is process wide, and every universe these specs build names this
// assembly. With classes running in parallel, a spec listening for its own universe could read
// another spec's event (e.g. a Generated universe instead of its Explicit one). The suite takes a
// few seconds, so running it serially is cheaper than attributing events per spec.
[assembly: Xunit.v3.Parallelization(Mode = Xunit.Sdk.ParallelMode.None)]
