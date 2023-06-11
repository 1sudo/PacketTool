using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class OutOfOrderPacket : SoePacket
	{
		public OutOfOrderPacket(int packetNum, Packet packet, bool preprocessed) : base("Out Of Order", 17, packetNum, packet, preprocessed, 2) { }

		internal override void BreakdownPacket() { }
	}
}
