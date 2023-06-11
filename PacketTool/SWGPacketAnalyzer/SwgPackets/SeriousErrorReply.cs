using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class SeriousErrorReply : SoePacket
	{
		public SeriousErrorReply(int packetNum, Packet packet, bool preprocessed) : base("SeriousErrorReply", 30, packetNum, packet, preprocessed, 2) { }

		internal override void BreakdownPacket() { }
	}
}
