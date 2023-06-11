using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class StandalonePacket : SoePacket
	{
		public StandalonePacket(int packetNum, Packet packet, bool preprocessed) : base("Standalone", 21, packetNum, packet, preprocessed, 1)
		{
		}

		internal override void BreakdownPacket()
		{
			int num = 0;
			if (this.hasFooter && this.doProcess)
			{
				num = 3;
			}
			SwgPacket item = PacketHandler.instance.createNewGamePacket(this.Data, 0, this.Data.Count - num, this.packetNumber);
			this.SWGPackets.Add(item);
		}

		public bool IsValid()
		{
			bool result;
			try
			{
				base.ParseInt(1);
				if (this.SoeOpcode > 0 && this.SoeOpcode < 30)
				{
					result = true;
				}
				else
				{
					result = false;
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}
	}
}
