using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class Acknowledge : SoePacket
	{
		public Acknowledge(int packetNum, Packet packet, bool preprocessed) : base("Acknowledge", 21, packetNum, packet, preprocessed, 2) { }

		internal override void BreakdownPacket()
		{
			this.sequenceNumber = (uint)base.ParseNetByteShort(2);
			SwgPacket swgpacket = PacketHandler.instance.createNewGamePacket(this.Data, 2, this.Data.Count - 2, this.packetNumber);
			swgpacket.Name = "Acknowledge";
			swgpacket.NodeName = swgpacket.Name;
			swgpacket.crc = 0U;
			swgpacket.myStruct.Clear();
			this.SWGPackets.Clear();
			Variable variable = new Variable();
			variable.index = 0;
			variable.setLength(2);
			variable.Name = "Sequence Number";
			variable.ShowValue = true;
			variable.ByteOrder = ByteOrder.NetByte;
			variable.Type = VariableType.Short;
			swgpacket.myStruct.Add(variable);
			this.SWGPackets.Add(swgpacket);
		}
	}
}
