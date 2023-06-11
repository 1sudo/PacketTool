using System.Windows;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class DataPacket : SoePacket
	{
		public DataPacket(int packetNum, Packet packet, bool preprocessed) : base("Data", 9, packetNum, packet, preprocessed, 2) { }

		internal override void BreakdownPacket()
		{
			this.sequenceNumber = (uint)base.ParseNetByteShort(2);
			this.HasMultipleInternalPackets = (base.ParseByte(4) == 0);
			if (!this.HasMultipleInternalPackets)
			{
				int length;
				if (this.hasFooter)
				{
					length = this.Data.Count - 7;
				}
				else
				{
					length = this.Data.Count - 4;
				}
				SwgPacket item = PacketHandler.instance.createNewGamePacket(this.Data, 4, length, this.packetNumber);
				this.SWGPackets.Add(item);
				return;
			}
			this.AddMultiplePackets();
		}

		private void AddMultiplePackets()
		{
			ushort num = base.ParseNetByteShort(4);
			if (num != 25)
			{
                MessageBox.Show("Error!", string.Concat(new object[]
				{
					"Unexpected Data Opcode in packet ",
					this.packetNumber,
					": ",
					num.ToString("X2")
				}));
			}
			int num2 = 6;
			int num3;
			while ((num3 = (int)base.ParseByte(num2)) > 1)
			{
				num2++;
				if (num3 == 255)
				{
					num3 = (int)base.ParseNetByteShort(num2);
					num2 += 2;
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
	}
}
