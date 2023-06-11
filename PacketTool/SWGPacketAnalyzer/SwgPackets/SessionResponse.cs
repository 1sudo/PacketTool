using System;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	internal class SessionResponse : SoePacket
	{
		public SessionResponse(int packetNum, Packet packet, bool preproccessed) : base("SessionResponse", 2, packetNum, packet, preproccessed, 2) { }

		internal override void BreakdownPacket()
		{
			SwgPacket swgpacket = PacketHandler.instance.createNewGamePacket(this.Data, 2, this.Data.Count - 2, this.packetNumber);
			swgpacket.Name = "SessionResponse";
			swgpacket.crc = 0U;
			swgpacket.myStruct.Clear();
			this.SWGPackets.Clear();
			Variable variable = new Variable();
			variable.index = 0;
			variable.setLength(4);
			variable.Name = "Connection ID";
			variable.ShowValue = true;
			variable.ByteOrder = ByteOrder.NetByte;
			variable.Type = VariableType.Int;
			swgpacket.myStruct.Add(variable);
			variable = new Variable();
			variable.index = 1;
			variable.setLength(4);
			variable.Name = "Session Key";
			variable.ShowInHex = true;
			variable.ShowValue = true;
			variable.ByteOrder = ByteOrder.NetByte;
			variable.Type = VariableType.Int;
			swgpacket.myStruct.Add(variable);
			this.SessionKey = base.ParseNetByteInt(6);
			variable = new Variable();
			variable.index = 2;
			variable.setLength(1);
			variable.Name = "CRC Length";
			variable.ShowValue = true;
			variable.ByteOrder = ByteOrder.NetByte;
			variable.Type = VariableType.Byte;
			swgpacket.myStruct.Add(variable);
			variable = new Variable();
			variable.index = 3;
			variable.setLength(1);
			variable.Name = "Encryption Method";
			variable.ShowValue = true;
			variable.ByteOrder = ByteOrder.NetByte;
			variable.Type = VariableType.Byte;
			swgpacket.myStruct.Add(variable);
			variable = new Variable();
			variable.index = 4;
			variable.setLength(1);
			variable.Name = "Seed Length";
			variable.ShowValue = true;
			variable.ByteOrder = ByteOrder.NetByte;
			variable.Type = VariableType.Byte;
			swgpacket.myStruct.Add(variable);
			variable = new Variable();
			variable.index = 5;
			variable.setLength(4);
			variable.Name = "UDP Size";
			variable.ShowValue = true;
			variable.ByteOrder = ByteOrder.NetByte;
			variable.Type = VariableType.Int;
			swgpacket.myStruct.Add(variable);
			this.SWGPackets.Add(swgpacket);
		}

		public uint SessionKey;
	}
}
