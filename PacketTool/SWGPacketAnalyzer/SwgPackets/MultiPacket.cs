using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using PacketDotNet;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class MultiPacket : SoePacket
	{
		public MultiPacket(int packetNum, Packet packet, bool preprocessed) : base("MultiPacket", 3, packetNum, packet, preprocessed, 2)
		{
			this.rawPacket = packet;
		}

		internal override void BreakdownPacket()
		{
			int num = 2;
			this.length = (int)base.ParseByte(num++);
			try
			{
				while (num < this.Data.Count && this.length >= 1 && num + this.length <= this.Data.Count)
				{
					SoePacket nextPacket = this.GetNextPacket(num, this.length);
					nextPacket.setPacketNumber(this.packetNumber);
					if (nextPacket != null)
					{
						this.SOEPacketList.Add(nextPacket);
					}
					else
					{
                        MessageBox.Show("Error!", "Error getting packet from multi");
					}
					num += this.length;
					if (num + 1 >= this.Data.Count - 3)
					{
						break;
					}
					this.length = (int)base.ParseByte(num++);
				}
			}
			catch
			{
			}
			foreach (SoePacket soepacket in this.SOEPacketList)
			{
				foreach (SwgPacket item in soepacket.SWGPackets)
				{
					this.SWGPackets.Add(item);
				}
			}
		}

		private SoePacket GetNextPacket(int offset, int length)
		{
			EthernetPacket ethernetPacket = null;
			try
			{
				string address = "FFFFFFFFFFFF";
				PhysicalAddress sourceHwAddress = PhysicalAddress.Parse(address);
				PhysicalAddress destinationHwAddress = PhysicalAddress.Parse(address);
				ethernetPacket = new EthernetPacket(sourceHwAddress, destinationHwAddress, EthernetType.IPv4);
				ethernetPacket.Type = EthernetType.IPv4;
				IPAddress sourceAddress = IPAddress.Parse("0.0.0.0");
				IPAddress destinationAddress = IPAddress.Parse("255.255.255.255");
				IPv4Packet pv4Packet = new IPv4Packet(sourceAddress, destinationAddress);
				UdpPacket udpPacket = new UdpPacket((ushort)this.sourcePort, (ushort)this.destinationPort);
				byte[] payloadData = this.Data.GetRange(offset, length).ToArray();
				pv4Packet.Version = IPVersion.IPv4;
				udpPacket.PayloadData = payloadData;
				pv4Packet.PayloadPacket = udpPacket;
				ethernetPacket.PayloadPacket = pv4Packet;
				udpPacket.UpdateUdpChecksum();
				pv4Packet.UpdateIPChecksum();
			}
			catch
			{
				return null;
			}
			return PacketHandler.instance.CreateNewSOEPacket(ethernetPacket, true, false);
		}

		private int length;
	}
}
