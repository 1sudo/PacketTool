using System;
using System.Collections.Generic;

namespace SwgPacketAnalyzer.packets
{
	internal class DeltasPacket : BaselinesPacket
	{
		public DeltasPacket(List<byte> Data, int offset, int length, int packetNum) : base(Data, offset, length, packetNum) { }
	}
}
