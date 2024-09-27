// Copyright (c) Event Store Ltd and/or licensed to Event Store Ltd under one or more agreements.
// Event Store Ltd licenses this file to you under the Event Store License v2 (see LICENSE.md).

using System;
using EventStore.LogV3;

namespace EventStore.Core.TransactionLog.LogRecords {
	public class LogV3EpochLogRecord : LogV3Record<RecordView<Raw.EpochHeader>>, ISystemLogRecord {
		public SystemRecordType SystemRecordType => SystemRecordType.Epoch;

		public LogV3EpochLogRecord(
			long logPosition,
			DateTime timeStamp,
			int epochNumber,
			Guid epochId,
			long prevEpochPosition,
			Guid leaderInstanceId) : base() {

			Record = RecordCreator.CreateEpochRecord(
				timeStamp: timeStamp,
				logPosition: logPosition,
				epochNumber: epochNumber,
				epochId: epochId,
				prevEpochPosition: prevEpochPosition,
				leaderInstanceId: leaderInstanceId);
		}

		public LogV3EpochLogRecord(ReadOnlyMemory<byte> bytes) : base() {
			Record = new RecordView<Raw.EpochHeader>(bytes);
		}

		public EpochRecord GetEpochRecord() => new EpochRecord(
			epochPosition: Record.Header.LogPosition,
			epochNumber: Record.SubHeader.EpochNumber,
			epochId: Record.Header.RecordId,
			prevEpochPosition: Record.SubHeader.PrevEpochPosition,
			timeStamp: Record.Header.TimeStamp,
			leaderInstanceId: Record.SubHeader.LeaderInstanceId);
	}
}
