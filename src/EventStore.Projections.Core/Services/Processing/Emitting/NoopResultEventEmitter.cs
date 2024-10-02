// Copyright (c) Event Store Ltd and/or licensed to Event Store Ltd under one or more agreements.
// Event Store Ltd licenses this file to you under the Event Store License v2 (see LICENSE.md).

using System;
using EventStore.Projections.Core.Services.Processing.Checkpointing;
using EventStore.Projections.Core.Services.Processing.Emitting.EmittedEvents;

namespace EventStore.Projections.Core.Services.Processing.Emitting;

public class NoopResultEventEmitter : IResultEventEmitter {
	public EmittedEventEnvelope[] ResultUpdated(string partition, string result, CheckpointTag at) {
		throw new NotSupportedException("No results are expected from the projection");
	}
}
