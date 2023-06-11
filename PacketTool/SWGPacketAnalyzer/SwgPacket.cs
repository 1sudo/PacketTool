using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using PacketTool.Models;
using PacketTool.ViewModels;
using SwgPacketAnalyzer.nodes;
using SwgPacketAnalyzer.SwgPackets;
using SwgPacketAnalyzer.variables;

namespace SwgPacketAnalyzer
{
	public class SwgPacket : Parser
	{
		public SwgPacket(List<byte> inData, int offset, int length, int num)
		{
			this.PacketNum = num;
			if (SwgPacket.PacketNames == null)
			{
				this.LoadPacketNames();
			}
			try
			{
				for (int i = offset; i < length + offset; i++)
				{
					this.Data.Add(inData[i]);
				}
			}
			catch
			{
				this.Data.Clear();
				for (int j = offset; j < inData.Count; j++)
				{
					this.Data.Add(inData[j]);
				}
			}
			this.ParsePacketName();
			this.SetupBreakDown();
			if (this.NodeName == "")
			{
				this.NodeName = "";
			}
		}

		public SwgPacket(SwgPacket packetStruct, int num) : this(packetStruct.Data, 0, packetStruct.Data.Count, num)
		{
			this.myStruct = packetStruct.myStruct;
			this.Name = packetStruct.Name;
		}

		private void LoadPacketNames()
		{
			if (!File.Exists("packetfunctions.txt"))
			{
                MessageBox.Show("Error!", "Error: Missing 'packetfunctions.txt' file");
				return;
			}
			this.ParseAndFilePacketFunctions();
		}

		private void ParseAndFilePacketFunctions()
		{
			if (!File.Exists("packetfunctions.txt"))
			{
				return;
			}
			SwgPacket.PacketNames = new Dictionary<uint, string>();
			TextReader textReader = new StreamReader("packetfunctions.txt");
			string text;
			while ((text = textReader.ReadLine()) != null)
			{
				string[] array = text.Split(new char[]
				{
					','
				});
				uint key = uint.Parse(array[0], NumberStyles.HexNumber);
				SwgPacket.PacketNames.Add(key, array[1].Trim());
			}
		}

		protected void ParsePacketName()
		{
			if (this.Data.Count < 6)
			{
				this.Name = "Packet Error";
				return;
			}
			this.PacketOpcode = base.ParseShort(0);
			this.crc = this.ParseCRC(2);
			if (SwgPacket.PacketNames.ContainsKey(this.crc))
			{
				this.Name = SwgPacket.PacketNames[this.crc];
			}
			else
			{
				this.Name = "Unknown";
			}
			if (MainWindowModel.PacketVariables.ContainsKey(this.Name))
			{
				this.myStruct = MainWindowModel.PacketVariables[this.Name];
			}
			string arg = " (Packet: " + this.PacketNum + ")";
			this.NodeName = string.Format("{0, -27} {1, 13}", this.Name, arg);
		}

		protected virtual uint ParseCRC(int p)
		{
			return base.ParseInt(p);
		}

		public virtual void SetupBreakDown()
		{
			if (MainWindowModel.PacketVariables.ContainsKey(this.Name))
			{
				this.myStruct = MainWindowModel.PacketVariables[this.Name];
			}
			if (this.myStruct == null)
			{
				double totalSeconds = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
				this.myStruct = new VariableDictionary(this.crc, this.Name, (long)totalSeconds);
				Variable variable = new Variable();
				variable.index = 0;
				variable.setLength(2);
				variable.setComplete(true);
				variable.Type = VariableType.Short;
				variable.ByteOrder = ByteOrder.HostByte;
				variable.Name = "Opcode";
				variable.ShowValue = true;
				this.myStruct.Add(variable);
				Variable variable2 = new Variable();
				variable2.index = 1;
				variable2.setLength(4);
				variable2.setComplete(true);
				variable2.Type = VariableType.Int;
				variable2.ByteOrder = ByteOrder.HostByte;
				variable2.Name = this.Name;
				variable2.Description = "";
				variable2.ShowValue = false;
				this.myStruct.Add(variable2);
			}
		}

		internal string[] GetLineNumbersForEditor()
		{
			List<string> list = new List<string>();
			int rawLineCount = base.GetRawLineCount(this.Data);
			for (int i = 0; i < rawLineCount; i++)
			{
				string item = (i * 16).ToString("X4") + ":";
				list.Add(item);
			}
			return list.ToArray();
		}

		internal string[] GetHexForEditor()
		{
			List<string> list = new List<string>();
			int rawLineCount = base.GetRawLineCount(this.Data);
			for (int i = 0; i < rawLineCount; i++)
			{
				string text = "";
				int lineByteCount = base.GetLineByteCount(this.Data, i);
				for (int j = 0; j < lineByteCount; j++)
				{
					text = text + this.Data[i * 16 + j].ToString("X2") + " ";
				}
				list.Add(text);
			}
			return list.ToArray();
		}

		internal string[] GetAsciiForEditor()
		{
			List<string> list = new List<string>();
			int rawLineCount = base.GetRawLineCount(this.Data);
			for (int i = 0; i < rawLineCount; i++)
			{
				int lineByteCount = base.GetLineByteCount(this.Data, i);
				string text = "";
				for (int j = 0; j < lineByteCount; j++)
				{
					char c = Convert.ToChar(this.Data[i * 16 + j]);
					if (c < '!' || c > '~')
					{
						c = '.';
					}
					text += c;
				}
				list.Add(text.Trim());
			}
			return list.ToArray();
		}

		internal string[] GetDisplayView()
		{
			if (this.PacketDisplay == null)
			{
				this.PacketDisplay = new List<string>();
				this.PacketDisplay.AddRange(base.GenerateRawView(this.Data));
				this.PacketDisplay.Add("");
			}
			return this.PacketDisplay.ToArray();
		}

		internal string[] GetPacketBreakdown()
		{
			List<string> list = new List<string>();
			int num = -1;
			bool flag = false;
			bool flag2 = false;
			ArrayList arrayList = new ArrayList();
			if (MainWindowModel.PacketVariables.ContainsKey(this.Name))
			{
				this.myStruct = MainWindowModel.PacketVariables[this.Name];
			}
			int num2 = 0;
			string line = "";
			string file = "";
			foreach (Variable variable in this.myStruct)
			{
				file = "";
				line = variable.Name;
				try
				{
					variable.currentvalue = this.ConvertBytesToValueString(variable.getType(), variable.getByteOrder(), variable.isHex(), this.GetRangeAsString(num2, variable.getLength()));
				}
				catch
				{
					variable.currentvalue = "n/a";
				}
				if (num != variable.listid)
				{
					flag2 = (num != -1 && variable.listid > num);
					if (variable.listindex != -1)
					{
						flag = true;
						num = variable.listid;
					}
					else
					{
						flag = false;
					}
				}
				if (flag2)
				{
					int num3 = int.Parse(this.myStruct[num].currentvalue);
					if (!flag)
					{
						num = -1;
					}
					list.Add("{");
					for (int i = 0; i < num3; i++)
					{
						foreach (object obj in arrayList)
						{
							Variable variable2 = (Variable)obj;
							line = variable.Name;
							list.Add("\t" + this.showVariable(variable2, file, line, num2));
							num2 += variable2.getLength();
						}
						list.Add("");
					}
					list.Add("}");
					arrayList.Clear();
				}
				if (flag)
				{
					arrayList.Add(variable);
				}
				else
				{
					list.Add(this.showVariable(variable, file, line, num2));
					num2 += variable.getLength();
				}
			}
			if (arrayList.Count > 0)
			{
				int num4 = int.Parse(this.myStruct[num].currentvalue);
				list.Add("{");
				for (int j = 0; j < num4; j++)
				{
					foreach (object obj2 in arrayList)
					{
						Variable variable3 = (Variable)obj2;
						line = variable3.Name;
						list.Add("\t" + this.showVariable(variable3, file, line, num2));
						num2 += variable3.getLength();
					}
					list.Add("");
				}
				list.Add("}");
				arrayList.Clear();
			}
			return list.ToArray();
		}

		private string showVariable(Variable myvar, string file, string line, int currentOffset)
		{
			if (myvar.ShowValue)
			{
				if (myvar.Type == VariableType.Int)
				{
					file = MainWindowModel.CrcLookup(this.ConvertBytesToCRC(this.GetRangeAsString(currentOffset, myvar.getLength())));
				}
				if (file != "")
				{
					line = line + " '" + file + "'";
				}
				else
				{
					if (myvar.getType() == VariableType.Ascii || myvar.getType() == VariableType.Unicode)
					{
						int num = 1;
						string s;
						if (myvar.getType() == VariableType.Ascii)
						{
							string rangeAsString = this.GetRangeAsString(currentOffset, 2);
							s = this.ConvertBytesToValueString(VariableType.Short, ByteOrder.HostByte, myvar.isHex(), rangeAsString);
						}
						else
						{
							string rangeAsString = this.GetRangeAsString(currentOffset, 4);
							s = this.ConvertBytesToValueString(VariableType.Int, ByteOrder.HostByte, myvar.isHex(), rangeAsString);
							num = 2;
						}
						try
						{
							myvar.setStringLength(int.Parse(s) * num);
						}
						catch
						{
						}
						if (myvar.getType() == VariableType.Ascii)
						{
							line = line + " '" + this.ConvertBytesToValueString(myvar.Type, myvar.ByteOrder, myvar.ShowInHex, this.GetRangeAsString(currentOffset + 2, myvar.getLength() - 2)) + "'";
						}
						else
						{
							line = line + " '" + this.ConvertBytesToValueString(myvar.Type, myvar.ByteOrder, myvar.ShowInHex, this.GetRangeAsString(currentOffset + 4, myvar.getLength() - 4)) + "'";
						}
					}
					else
					{
						line = line + " '" + this.ConvertBytesToValueString(myvar.Type, myvar.ByteOrder, myvar.ShowInHex, this.GetRangeAsString(currentOffset, myvar.getLength())) + "'";
					}
					try
					{
						myvar.currentvalue = this.ConvertBytesToValueString(myvar.getType(), myvar.getByteOrder(), myvar.isHex(), this.GetRangeAsString(currentOffset, myvar.getLength()));
					}
					catch
					{
					}
				}
			}
			if (!string.IsNullOrEmpty(myvar.Description))
			{
				line = line + "      // " + myvar.Description;
			}
			return line;
		}

		public string ConvertBytesToValueString(VariableType variableType, ByteOrder order, bool hex, string byteslist)
		{
			if (string.IsNullOrEmpty(byteslist))
			{
				return "";
			}
			string[] array = byteslist.Trim().Split(new char[]
			{
				' '
			});
			List<byte> list = new List<byte>();
			if (hex)
			{
				string text = "";
				foreach (string str in array)
				{
					text += str;
				}
				return text;
			}
			foreach (string value in array)
			{
				if (order == ByteOrder.NetByte)
				{
					list.Insert(0, Convert.ToByte(value, 16));
				}
				else
				{
					list.Add(Convert.ToByte(value, 16));
				}
			}
			switch (variableType)
			{
			case VariableType.Byte:
				return list[0].ToString();
			case VariableType.Short:
				return BitConverter.ToUInt16(list.ToArray(), 0).ToString();
			case VariableType.Int:
				return BitConverter.ToUInt32(list.ToArray(), 0).ToString();
			case VariableType.Float:
				return BitConverter.ToSingle(list.ToArray(), 0).ToString();
			case VariableType.Long:
				return BitConverter.ToUInt64(list.ToArray(), 0).ToString();
			case VariableType.Ascii:
				return Encoding.ASCII.GetString(list.ToArray());
			case VariableType.Unicode:
				return Encoding.Unicode.GetString(list.ToArray());
			default:
				return "";
			}
		}

		private uint ConvertBytesToCRC(string byteslist)
		{
			string[] array = byteslist.Trim().Split(new char[]
			{
				' '
			});
			List<byte> list = new List<byte>();
			foreach (string value in array)
			{
				list.Add(Convert.ToByte(value, 16));
			}
			if (list.Count == 4)
			{
				return BitConverter.ToUInt32(list.ToArray(), 0);
			}
			return 0U;
		}

		internal string GetRangeAsString(int offset, int length)
		{
			string text = "";
			if (offset + length > this.Data.Count)
			{
				length = this.Data.Count - offset;
			}
			try
			{
				List<byte> range = this.Data.GetRange(offset, length);
				foreach (byte b in range)
				{
					text = text + b.ToString("X2") + " ";
				}
			}
			catch
			{
			}
			return text;
		}

		public static Dictionary<uint, string> PacketNames;

		public VariableDictionary myStruct;

		public IPacketNode MyNode;

		public List<string> ExpectedResponses = new();

		public ushort PacketOpcode;

		public string Name;

		public string NodeName;

		public uint crc;

		public int PacketNum;

		public SoePacket mySOEPacket;

		private List<string> PacketDisplay;
	}
}
