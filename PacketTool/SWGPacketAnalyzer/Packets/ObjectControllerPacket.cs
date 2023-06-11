using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using PacketTool.Models;

namespace SwgPacketAnalyzer.packets
{
	internal class ObjectControllerPacket : SwgPacket
	{
		public ObjectControllerPacket(List<byte> Data, int offset, int length, int packetNum) : base(Data, offset, length, packetNum) { }

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
				variable.setLength(4);
				variable.setComplete(false);
				variable.Type = VariableType.Int;
				variable.ByteOrder = ByteOrder.HostByte;
				variable.Name = "Unknown";
				variable.ShowValue = true;
				this.myStruct.Add(variable);
				variable = new Variable();
				variable.index = 3;
				variable.setLength(4);
				variable.setComplete(true);
				variable.Type = VariableType.Int;
				variable.ByteOrder = ByteOrder.HostByte;
				variable.Name = "Header";
				variable.ShowValue = true;
				this.myStruct.Add(variable);
				variable = new Variable();
				variable.index = 4;
				variable.setLength(8);
				variable.setComplete(true);
				variable.Type = VariableType.Long;
				variable.ByteOrder = ByteOrder.HostByte;
				variable.Name = "Object ID";
				variable.ShowValue = true;
				variable.ShowInHex = true;
				this.myStruct.Add(variable);
				variable = new Variable();
				variable.index = 5;
				variable.setLength(4);
				variable.setComplete(true);
				variable.Type = VariableType.Int;
				variable.ByteOrder = ByteOrder.HostByte;
				variable.Name = "Tick Count";
				variable.ShowValue = true;
				this.myStruct.Add(variable);
			}
			this.SetObjectControllerName();
			if (this.Name.ToLower().Equals("commandqueueenqueue"))
			{
				this.SetCommandName();
			}
		}

		private void SetCommandName()
		{
			if (ObjectControllerPacket.CommandPackets == null)
			{
				this.LoadCommandTypes();
			}
			uint key = base.ParseInt(30);
			if (ObjectControllerPacket.CommandPackets.ContainsKey(key))
			{
				this.Name = ObjectControllerPacket.CommandPackets[key] + "Command";
				return;
			}
			this.Name = "Unknown Command (" + key.ToString("X4") + ")";
		}

		private void SetObjectControllerName()
		{
			if (ObjectControllerPacket.ObjectControllerPacketTypes == null)
			{
				this.LoadObjectControllerTypes();
			}
			uint key = base.ParseInt(10);
			this.crc ^= base.ParseInt(10);
			if (ObjectControllerPacket.ObjectControllerPacketTypes.ContainsKey(key))
			{
				this.Name = ObjectControllerPacket.ObjectControllerPacketTypes[key];
				return;
			}
			this.Name = "Unknown Obj Controller (" + key.ToString("X4") + ")";
		}

		private void LoadObjectControllerTypes()
		{
			if (!File.Exists("objectcontrollers.txt"))
			{
                MessageBox.Show("Error!", "Error: Missing 'objectcontrollers.txt' file");
				return;
			}
			this.ParseAndFileObjectControllers();
		}

		private void ParseAndFileObjectControllers()
		{
			if (!File.Exists("objectcontrollers.txt"))
			{
				return;
			}
			ObjectControllerPacket.ObjectControllerPacketTypes = new Dictionary<uint, string>();
			TextReader textReader = new StreamReader("objectcontrollers.txt");
			string text;
			while ((text = textReader.ReadLine()) != null)
			{
				string[] array = text.Split(new char[]
				{
					';'
				});
				uint key = uint.Parse(array[0], NumberStyles.HexNumber);
				ObjectControllerPacket.ObjectControllerPacketTypes.Add(key, array[1].Trim());
			}
		}

		private void LoadCommandTypes()
		{
			if (!File.Exists("commands.txt"))
			{
				MessageBox.Show("Error!", "Error: Missing 'commands.txt' file");
				return;
			}
			this.ParseAndFileCommands();
		}

		private void ParseAndFileCommands()
		{
			if (!File.Exists("commands.txt"))
			{
				return;
			}
			ObjectControllerPacket.CommandPackets = new Dictionary<uint, string>();
			TextReader textReader = new StreamReader("commands.txt");
			string text;
			while ((text = textReader.ReadLine()) != null)
			{
				string[] array = text.Split(new char[]
				{
					';'
				});
				uint key = uint.Parse(array[1], NumberStyles.HexNumber);
				ObjectControllerPacket.CommandPackets.Add(key, array[0].Trim());
			}
		}

		private static Dictionary<uint, string> ObjectControllerPacketTypes;
		private static Dictionary<uint, string> CommandPackets;
	}
}
