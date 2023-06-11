using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class UnknownPacket : SoePacket
	{
		public UnknownPacket(int packetNum, Packet packet, bool preprocessed) : base("UnknownPacket", 0, packetNum, packet, preprocessed, 2) { }

		internal override void BreakdownPacket() { }
	}
}
