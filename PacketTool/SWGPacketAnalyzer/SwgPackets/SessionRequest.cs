using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class SessionRequest : SoePacket
	{
		public SessionRequest(int packetNum, Packet packet, bool preprocessed) : base("SessionRequest", 1, packetNum, packet, preprocessed, 2)
		{
			this.SoeOpcode = 1;
		}

		internal override void BreakdownPacket()
		{
			SwgPacket swgpacket = PacketHandler.instance.createNewGamePacket(this.Data, 2, this.Data.Count - 2, this.packetNumber);
			swgpacket.Name = "SessionRequest";
			swgpacket.crc = 0U;
			swgpacket.myStruct.Clear();
			this.SWGPackets.Clear();
			Variable variable = new Variable();
			variable.index = 0;
			variable.setLength(4);
			variable.Name = "CRC Length";
			variable.ShowValue = true;
			variable.ByteOrder = ByteOrder.NetByte;
			variable.Type = VariableType.Int;
			swgpacket.myStruct.Add(variable);
			variable = new Variable();
			variable.index = 1;
			variable.setLength(4);
			variable.Name = "Connection ID";
			variable.ShowValue = true;
			variable.ByteOrder = ByteOrder.NetByte;
			variable.Type = VariableType.Int;
			swgpacket.myStruct.Add(variable);
			variable = new Variable();
			variable.index = 2;
			variable.setLength(4);
			variable.Name = "Max UDP Size";
			variable.ShowValue = true;
			variable.ByteOrder = ByteOrder.NetByte;
			variable.Type = VariableType.Int;
			swgpacket.myStruct.Add(variable);
			this.SWGPackets.Add(swgpacket);
			swgpacket.ExpectedResponses.Add("SessionResponse");
		}
	}
}
