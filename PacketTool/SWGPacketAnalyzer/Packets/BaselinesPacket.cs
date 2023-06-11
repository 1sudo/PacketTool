using System;
using System.Collections.Generic;
using PacketTool.Models;

namespace SwgPacketAnalyzer.packets
{
	internal class BaselinesPacket : SwgPacket
	{
		public BaselinesPacket(List<byte> Data, int offset, int length, int packetNum) : base(Data, offset, length, packetNum) { }

		public override void SetupBreakDown()
		{
			base.SetupBreakDown();
			if (MainWindowModel.PacketVariables.ContainsKey(this.Name))
			{
				this.myStruct = MainWindowModel.PacketVariables[this.Name];
			}
			if (this.myStruct == null)
			{
				Variable variable = new Variable();
				variable.index = 2;
				variable.setLength(8);
				variable.setComplete(true);
				variable.Type = VariableType.Long;
				variable.ByteOrder = ByteOrder.HostByte;
				variable.Name = "Object ID";
				variable.ShowValue = true;
				this.myStruct.Add(variable);
				variable = new Variable();
				variable.index = 3;
				variable.setLength(4);
				variable.setComplete(true);
				variable.Type = VariableType.Int;
				variable.ByteOrder = ByteOrder.HostByte;
				variable.Name = "Object Type";
				variable.ShowValue = false;
				this.myStruct.Add(variable);
				variable = new Variable();
				variable.index = 4;
				variable.setLength(1);
				variable.setComplete(true);
				variable.Type = VariableType.Byte;
				variable.ByteOrder = ByteOrder.HostByte;
				variable.Name = "Type Number";
				variable.ShowValue = true;
				variable.ShowInHex = true;
				this.myStruct.Add(variable);
				variable = new Variable();
				variable.index = 5;
				variable.setLength(4);
				variable.setComplete(true);
				variable.Type = VariableType.Int;
				variable.ByteOrder = ByteOrder.HostByte;
				variable.Name = "Data Size";
				variable.ShowValue = true;
				this.myStruct.Add(variable);
			}
			this.SetBaselineName();
		}

		private void SetBaselineName()
		{
			List<byte> range = this.Data.GetRange(14, 4);
			byte b = base.ParseByte(18);
			this.crc ^= base.ParseInt(14);
			this.crc += (uint)b;
			string name = this.Name;
			string text;
			if (name.Contains("Baseline"))
			{
				text = "Baseline";
			}
			else
			{
				text = "Delta";
			}
			this.Name = "";
			this.Name += Convert.ToChar(range[range.Count - 1]);
			this.Name += Convert.ToChar(range[range.Count - 2]);
			this.Name += Convert.ToChar(range[range.Count - 3]);
			this.Name += Convert.ToChar(range[range.Count - 4]);
			object name2 = this.Name;
			this.Name = string.Concat(new object[]
			{
				name2,
				" ",
				b,
				" ",
				text
			});
		}
	}
}
