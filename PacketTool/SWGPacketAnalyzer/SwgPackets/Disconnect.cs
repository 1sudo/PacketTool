using System;
using System.Collections.Generic;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class Disconnect : SoePacket
	{
		public Disconnect(int packetNum, Packet packet, bool preproccessed) : base("Disconnect", 5, packetNum, packet, preproccessed, 2) { }

		internal override void BreakdownPacket()
		{
			if (this.Data.Count >= 8)
			{
				this.connectionId = base.ParseNetByteInt(2);
				this.reasonId = base.ParseNetByteShort(6);
			}
		}

		internal override string[] GetPacketBreakdown(bool update = false)
		{
			return new List<string>
			{
				this.name,
				"",
				this.connectionId + " - Connection ID",
				this.reasonId + " - Disconnect Reason"
			}.ToArray();
		}

		private uint connectionId;
		private ushort reasonId;
	}
}
