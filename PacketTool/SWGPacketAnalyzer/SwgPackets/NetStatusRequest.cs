using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class NetStatusRequest : SoePacket
	{
		public NetStatusRequest(int packetNum, Packet packet, bool preprocessed) : base("ClientNetStatusUpdate", 7, packetNum, packet, preprocessed, 2) { }

		internal override void BreakdownPacket() { }

		private short clientTickCount;
		private int lastUpdate;
		private int averageUpdate;
		private int shortestUpdate;
		private int longestUpdate;
		private int lastServerUpdate;
		private long packetsSent;
		private long packetsRecieved;
	}
}
