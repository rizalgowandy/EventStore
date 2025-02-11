using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace EventStore.Core.DataStructures.ProbabilisticFilter.MemoryMappedFileBloomFilter {
	[StructLayout(LayoutKind.Explicit, Size = Size, Pack = 1)]
	public struct Header {
		internal const byte CurrentVersion = 1;
		internal const int Size = 16;

		[FieldOffset(0)] private byte _version;
		[FieldOffset(4)] private int _corruptionRebuildCount;
		[FieldOffset(8)] private long _numBits;


		public byte Version {
			get => _version;
			set => _version = value;
		}

		public int CorruptionRebuildCount {
			get => _corruptionRebuildCount;
			set => _corruptionRebuildCount = value;
		}

		public long NumBits {
			get => _numBits;
			set => _numBits = value;
		}

		public static Header ReadFrom(MemoryMappedFile mmf) {
			try {
				//read the version first
				using (var headerAccessor = mmf.CreateViewAccessor(0, 1, MemoryMappedFileAccess.Read)) {
					byte version = headerAccessor.ReadByte(0);
					if (version != CurrentVersion) {
						throw new CorruptedFileException($"Unsupported version: {version}");
					}
				}

				//then the full header
				var headerBytes = new byte[Size].AsSpan();
				using (var headerAccessor = mmf.CreateViewStream(0, Size, MemoryMappedFileAccess.Read)) {
					int read = headerAccessor.Read(headerBytes);
					if (read != Size) {
						throw new CorruptedFileException(
							$"File header size ({read} bytes) does not match expected header size ({Size} bytes)");
					}
				}

				return MemoryMarshal.AsRef<Header>(headerBytes);
			} catch(Exception exc) when (!(exc is CorruptedFileException)) {
				throw new CorruptedFileException("Failed to read the header");
			}
		}

		public void WriteTo(MemoryMappedFile mmf) {
			var span = MemoryMarshal.CreateReadOnlySpan(ref this, 1);
			var headerBytes = MemoryMarshal.Cast<Header, byte>(span);
			using var headerAccessor = mmf.CreateViewStream(0, Size, MemoryMappedFileAccess.Write);
			headerAccessor.Write(headerBytes);
			headerAccessor.Flush();
		}
	}
}
