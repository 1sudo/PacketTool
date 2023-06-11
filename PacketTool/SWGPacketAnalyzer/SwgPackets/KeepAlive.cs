using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class KeepAlive : SoePacket
	{
		public KeepAlive(int packetNum, Packet packet, bool preproccessed) : base("KeepAlive", 6, packetNum, packet, preproccessed, 2) { }

		internal override void BreakdownPacket() { }
	}
}
