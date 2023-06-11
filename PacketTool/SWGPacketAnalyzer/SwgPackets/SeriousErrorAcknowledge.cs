using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class SeriousErrorAcknowledge : SoePacket
	{
		public SeriousErrorAcknowledge(int packetNum, Packet packet, bool preprocessed) : base("SeriousErrorAcknowledge", 29, packetNum, packet, preprocessed, 2) { }

		internal override void BreakdownPacket() { }
	}
}
