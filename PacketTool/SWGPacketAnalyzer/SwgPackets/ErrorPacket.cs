using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class ErrorPacket : SoePacket
	{
		public ErrorPacket(int packetNum, Packet packet, bool preprocessed) : base("ErrorPacket", 0, packetNum, packet, preprocessed, 2) { }

		internal override void BreakdownPacket() { }
	}
}
