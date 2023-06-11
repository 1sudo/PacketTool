using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class NetStatusResponse : SoePacket
	{
		public NetStatusResponse(int packetNum, Packet packet, bool preprocessed) : base("ServerNetStatusUpdate", 7, packetNum, packet, preprocessed, 2) { }

		internal override void BreakdownPacket() { }

		private short clientTickCount;
		private short serverTickCount;
		private long clientPacketsSent;
		private long serverPacketsSent;
		private long serverPacketsRecieved;
	}
}
