using System.Collections.Generic;
using System.Windows;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class FragmentedPacket : SoePacket
	{
		public FragmentedPacket(int packetNum, Packet packet, bool preprocessed) : base("Fragmented", 13, packetNum, packet, preprocessed, 2)
		{
			this.sequenceNumber = (uint)base.ParseNetByteShort(2);
			if (this.Data.Count < 18)
			{
				this.initialPacket = false;
				this.packetCount = -1;
				return;
			}
			ushort num = base.ParseShort(8);
			uint key = base.ParseInt(10);
			if (num > 0 && num < 30 && SwgPacket.PacketNames.ContainsKey(key))
			{
				this.initialPacket = true;
				this.totalSize = base.ParseNetByteInt(4);
				foreach (byte item in this.Data)
				{
					this.originalBytes.Add(item);
				}
				this.packetCount = (int)((ulong)(this.totalSize - 485U) / (ulong)((long)this.maxContribution)) + 1 + 1;
				return;
			}
			this.initialPacket = false;
			this.packetCount = -1;
		}

		public bool isComplete()
		{
			return this.packetCount == this.packetList.Count + 1;
		}

		internal override void BreakdownPacket()
		{
			if (!this.isComplete())
			{
				return;
			}
			this.HasMultipleInternalPackets = (base.ParseShort(8) == 25);
			if (!this.HasMultipleInternalPackets)
			{
				SwgPacket item = PacketHandler.instance.createNewGamePacket(this.Data, 8, this.Data.Count - 11, this.packetNumber);
				this.SWGPackets.Add(item);
			}
			else
			{
				this.AddMultiplePackets();
			}
			foreach (SwgPacket swgpacket in this.SWGPackets)
			{
				swgpacket.mySOEPacket = this;
			}
		}

		private void AddMultiplePackets()
		{
			ushort num = base.ParseNetByteShort(8);
			if (num != 25)
			{
                MessageBox.Show("Error!", string.Concat(new object[]
				{
					"Unexpected Opcode in fragmented packet ",
					this.packetNumber,
					": ",
					num.ToString()
				}));
			}
			int num2 = 6;
			int num3;
			while ((num3 = (int)base.ParseByte(num2)) > 1)
			{
				num2++;
				if (num3 == 255)
				{
					if (base.ParseByte(num2) == 1)
					{
						num2++;
						num3 += (int)(base.ParseByte(num2) + 1);
					}
					else
					{
						num2++;
						num3 = (int)base.ParseByte(num2);
					}
					num2++;
				}
				SwgPacket item = PacketHandler.instance.createNewGamePacket(this.Data, num2, num3, this.packetNumber);
				this.SWGPackets.Add(item);
				num2 += num3;
				if (num2 >= this.Data.Count)
				{
					this.name += " Error";
					return;
				}
			}
		}

		internal void AddFragment(FragmentedPacket fragmentedPacket)
		{
			lock (this)
			{
				this.packetList.Add(fragmentedPacket);
				if (this.isComplete())
				{
					this.CombineFragments();
				}
			}
		}

		private void CombineFragments()
		{
			foreach (FragmentedPacket fragmentedPacket in this.packetList)
			{
				this.Data.InsertRange(this.Data.Count - 3, fragmentedPacket.Data.GetRange(4, fragmentedPacket.Data.Count - 7));
			}
		}

		internal bool isInitial()
		{
			return this.initialPacket;
		}

		internal int getNeededCount()
		{
			return this.packetCount;
		}

		protected override string[] GenerateDisplayView()
		{
			if (this.initialPacket)
			{
				return base.GenerateRawView(this.originalBytes);
			}
			return base.GenerateRawView(this.Data);
		}

		internal List<FragmentedPacket> getPacketList()
		{
			return this.packetList;
		}

		private uint totalSize;
		private int maxContribution = 489;
		private int packetCount;
		private bool initialPacket;
		private List<FragmentedPacket> packetList = new List<FragmentedPacket>();
		private List<byte> originalBytes = new List<byte>();
	}
}
