﻿using EventStore.Core.Data;
using EventStore.Core.Tests.TransactionLog.Scavenging.Helpers;
using NUnit.Framework;
using ReadStreamResult = EventStore.Core.Services.Storage.ReaderIndex.ReadStreamResult;

namespace EventStore.Core.Tests.Services.Storage.Metastreams {
	[TestFixture(typeof(LogFormat.V2), typeof(string))]
	[TestFixture(typeof(LogFormat.V3), typeof(long))]
	public class
		when_having_multiple_metaevents_in_metastream_and_read_index_is_set_to_keep_just_last<TLogFormat, TStreamId>
		: SimpleDbTestScenario<TLogFormat, TStreamId> {
		protected override DbResult CreateDb(TFChunkDbCreationHelper<TLogFormat, TStreamId> dbCreator) {
			return dbCreator.Chunk(Rec.Prepare("$$test", "0", streamMetadata: new StreamMetadata(maxCount: 10)),
					Rec.Prepare("$$test", "1", streamMetadata: new StreamMetadata(maxCount: 9)),
					Rec.Prepare("$$test", "2", streamMetadata: new StreamMetadata(maxCount: 8)),
					Rec.Prepare("$$test", "3", streamMetadata: new StreamMetadata(maxCount: 7)),
					Rec.Prepare("$$test", "4", streamMetadata: new StreamMetadata(maxCount: 6)))
				.CreateDb();
		}

		[Test]
		public void last_event_read_returns_correct_event() {
			var res = ReadIndex.ReadEvent("$$test", -1);
			Assert.AreEqual(ReadEventResult.Success, res.Result);
			Assert.AreEqual("4", res.Record.EventType);
		}

		[Test]
		public void last_event_stream_number_is_correct() {
			Assert.AreEqual(4, ReadIndex.GetStreamLastEventNumber("$$test"));
		}

		[Test]
		public void single_event_read_returns_only_last_event() {
			Assert.AreEqual(ReadEventResult.NotFound, ReadIndex.ReadEvent("$$test", 0).Result);
			Assert.AreEqual(ReadEventResult.NotFound, ReadIndex.ReadEvent("$$test", 1).Result);
			Assert.AreEqual(ReadEventResult.NotFound, ReadIndex.ReadEvent("$$test", 2).Result);
			Assert.AreEqual(ReadEventResult.NotFound, ReadIndex.ReadEvent("$$test", 3).Result);

			var res = ReadIndex.ReadEvent("$$test", 4);
			Assert.AreEqual(ReadEventResult.Success, res.Result);
			Assert.AreEqual("4", res.Record.EventType);
		}

		[Test]
		public void stream_read_forward_returns_only_last_event() {
			var res = ReadIndex.ReadStreamEventsForward("$$test", 0, 100);
			Assert.AreEqual(ReadStreamResult.Success, res.Result);
			Assert.AreEqual(1, res.Records.Length);
			Assert.AreEqual("4", res.Records[0].EventType);
		}

		[Test]
		public void stream_read_backward_returns_only_last_event() {
			var res = ReadIndex.ReadStreamEventsBackward("$$test", -1, 100);
			Assert.AreEqual(ReadStreamResult.Success, res.Result);
			Assert.AreEqual(1, res.Records.Length);
			Assert.AreEqual("4", res.Records[0].EventType);
		}

		[Test]
		public void metastream_metadata_is_correct() {
			var metadata = ReadIndex.GetStreamMetadata("$$test");
			Assert.AreEqual(1, metadata.MaxCount);
			Assert.AreEqual(null, metadata.MaxAge);
		}

		[Test]
		public void original_stream_metadata_is_taken_from_last_metaevent() {
			var metadata = ReadIndex.GetStreamMetadata("test");
			Assert.AreEqual(6, metadata.MaxCount);
			Assert.AreEqual(null, metadata.MaxAge);
		}
	}
}
