using System;
using System.Collections.Generic;
using System.Net;
using ComponentAce.Compression.Libs.zlib;
using PacketDotNet;
using SharpPcap;
using SwgPacketAnalyzer.nodes;

namespace SwgPacketAnalyzer.SwgPackets
{
	public class SoePacket : Parser
	{
		public bool isSequenced()
		{
			return this.sequenceNumber > 0U;
		}

		public SoePacket(string n, ushort opcode, int packetnum, Packet packet, bool preprocessed, int offset = 2)
		{
			this.SoeOpcode = opcode;
			this.packetNumber = packetnum;
			this.rawPacket = packet;
			this.name = n;
			this.extractPacketData(packet);
			if (!preprocessed && this.verifyPacket())
			{
				this.hasFooter = true;
				if (this.Encrypted)
				{
					this.Decrypt(offset, this.Data.Count - 1 - offset);
				}
				this.Compressed = (base.ParseByte(this.Data.Count - 3) == 1);
				if (this.Compressed)
				{
					this.Decompress(offset, this.Data.Count - 2 - offset);
				}
			}
			else
			{
				this.hasFooter = false;
			}
			try
			{
				this.BreakdownPacket();
			}
			catch
			{
			}
			foreach (SwgPacket swgpacket in this.SWGPackets)
			{
				swgpacket.mySOEPacket = this;
			}
		}

		private bool verifyPacket()
		{
			if (this is SessionRequest || this is SessionResponse || this is KeepAlive || this is UnknownPacket)
			{
				return true;
			}
			if (Crc32.verifyPacket(this, PacketHandler.settings.SWGKey, 2))
			{
				this.Encrypted = true;
				return true;
			}
			this.Encrypted = false;
			this.Compressed = false;
			return false;
		}

		internal void extractPacketData(Packet packet)
		{
			IPPacket encapsulated = packet.Extract<IPPacket>();
			UdpPacket encapsulated2 = packet.Extract<UdpPacket>();

			if (encapsulated == null || encapsulated2 == null)
			{
				return;
			}
			this.Data.AddRange(encapsulated2.PayloadData);
			this.sourceAddress = encapsulated.SourceAddress;
			this.destinationAddress = encapsulated.DestinationAddress;
			string a = this.sourceAddress.ToString();
			if (a == "0.0.0.0")
			{
				this.doProcess = false;
				this.packetOrigin = PacketOrigin.Server;
			}
			else if (a == PacketHandler.settings.ServerAddress)
			{
				this.packetOrigin = PacketOrigin.Server;
			}
			else
			{
				this.packetOrigin = PacketOrigin.Client;
			}
			this.sourcePort = (int)encapsulated2.SourcePort;
			this.destinationPort = (int)encapsulated2.DestinationPort;
			// TODO - does this shit matter?
			// this.time = packet.Timeval;
			if ((ulong)PacketHandler.settings.loginPort == (ulong)((long)this.destinationPort) || (ulong)PacketHandler.settings.loginPort == (ulong)((long)this.sourcePort) || (ulong)PacketHandler.settings.defaultLoginPort == (ulong)((long)this.destinationPort) || (ulong)PacketHandler.settings.defaultLoginPort == (ulong)((long)this.sourcePort))
			{
				this.serverOrigin = "Login";
				return;
			}
			if ((ulong)PacketHandler.settings.zonePort == (ulong)((long)this.destinationPort) || (ulong)PacketHandler.settings.zonePort == (ulong)((long)this.sourcePort) || (ulong)PacketHandler.settings.defaultZonePort == (ulong)((long)this.destinationPort) || (ulong)PacketHandler.settings.defaultZonePort == (ulong)((long)this.sourcePort))
			{
				this.serverOrigin = "Zone";
				return;
			}
			if ((ulong)PacketHandler.settings.pingPort == (ulong)((long)this.destinationPort) || (ulong)PacketHandler.settings.pingPort == (ulong)((long)this.sourcePort) || (ulong)PacketHandler.settings.defaultPingPort == (ulong)((long)this.destinationPort) || (ulong)PacketHandler.settings.defaultPingPort == (ulong)((long)this.sourcePort))
			{
				this.serverOrigin = "Ping";
			}
		}

		internal bool IsLocalIpAddress(string host)
		{
			try
			{
				IPAddress[] hostAddresses = Dns.GetHostAddresses(host);
				IPAddress[] hostAddresses2 = Dns.GetHostAddresses(Dns.GetHostName());
				foreach (IPAddress ipaddress in hostAddresses)
				{
					if (IPAddress.IsLoopback(ipaddress))
					{
						return true;
					}
					foreach (IPAddress obj in hostAddresses2)
					{
						if (ipaddress.Equals(obj))
						{
							return true;
						}
					}
				}
			}
			catch
			{
			}
			return false;
		}

		internal virtual void BreakdownPacket()
		{
		}

		internal void Decrypt(int offset, int length)
		{
			List<byte> list = new List<byte>();
			list.AddRange(BitConverter.GetBytes(PacketHandler.settings.SWGKey));
			int num = (length - 1) / 4;
			int num2 = (length - 1) % 4;
			for (int i = 0; i < num; i++)
			{
				uint value = base.ParseInt(i * 4 + offset);
				for (int j = 0; j < 4; j++)
				{
					byte value2 = (byte)(this.Data[j + i * 4 + offset] ^ list[j]);
					this.Data[j + i * 4 + offset] = value2;
				}
				list.Clear();
				list.AddRange(BitConverter.GetBytes(value));
			}
			for (int k = 0; k < num2; k++)
			{
				byte value2 = (byte)(this.Data[offset + num * 4 + k] ^ list[0]);
				this.Data[offset + num * 4 + k] = value2;
			}
		}

		internal void Decompress(int offset, int length)
		{
			byte[] array = new byte[this.Data.Count];
			this.Data.CopyTo(0, array, 0, this.Data.Count);
			byte[] array2 = new byte[800];
			ZStream zstream = new ZStream();
			zstream.avail_in = 0;
			zstream.inflateInit();
			zstream.next_in = array;
			zstream.next_in_index = 2;
			zstream.avail_in = array.Length - 4;
			zstream.next_out = array2;
			zstream.avail_out = 800;
			if (zstream.inflate(4) != -3)
			{
				long total_out = zstream.total_out;
				zstream.inflateEnd();
				this.Data.Clear();
				this.Data.Add(array[0]);
				this.Data.Add(array[1]);
				int num = 0;
				while ((long)num < total_out)
				{
					this.Data.Add(array2[num]);
					num++;
				}
				this.Data.Add(array[array.Length - 3]);
				this.Data.Add(array[array.Length - 2]);
				this.Data.Add(array[array.Length - 1]);
			}
		}

		internal override string[] GetPacketBreakdown(bool update = false)
		{
			List<string> list = new List<string>();
			list.Add("*********************************************");
			list.Add("");
			foreach (SwgPacket swgpacket in this.SWGPackets)
			{
				list.AddRange(swgpacket.GetPacketBreakdown(update));
				list.Add("");
				list.Add("*********************************************");
				list.Add("");
			}
			return list.ToArray();
		}

		internal string[] GetDisplayView()
		{
			if (this.PacketDisplay == null)
			{
				this.PacketDisplay = new List<string>();
				this.PacketDisplay.Add(string.Concat(new object[]
				{
					"Packet : ",
					this.packetNumber,
					" Time: ",
					this.time.Date.ToLocalTime().Hour.ToString("D2"),
					":",
					this.time.Date.ToLocalTime().Minute.ToString("D2"),
					":",
					this.time.Date.ToLocalTime().Second.ToString("D2"),
					":",
					this.time.MicroSeconds.ToString("D6")
				}));
				string text = (this.packetOrigin == PacketOrigin.Client) ? "(Client->Server)" : "(Server->Client)";
				this.PacketDisplay.Add(string.Concat(new object[]
				{
					this.sourceAddress,
					":",
					this.sourcePort,
					" -> ",
					this.destinationAddress,
					":",
					this.destinationPort,
					" ",
					text
				}));
				this.PacketDisplay.Add(string.Concat(new object[]
				{
					this.serverOrigin,
					": ",
					this.name,
					": Enc: ",
					this.Encrypted.ToString()[0],
					" Comp: ",
					this.Compressed.ToString()[0]
				}));
				this.PacketDisplay.Add("");
				this.PacketDisplay.AddRange(this.GenerateDisplayView());
				this.PacketDisplay.Add("");
			}
			return this.PacketDisplay.ToArray();
		}

		protected virtual string[] GenerateDisplayView()
		{
			return base.GenerateRawView(this.Data);
		}

		public string getName()
		{
			return this.name;
		}

		public int getPacketNumber()
		{
			return this.packetNumber;
		}

		public string getDestinationAddress()
		{
			return this.destinationAddress.ToString();
		}

		public PacketOrigin getPacketOrigin()
		{
			return this.packetOrigin;
		}

		public string getServerOrigin()
		{
			return this.serverOrigin;
		}

		public uint getSequenceNumber()
		{
			return this.sequenceNumber;
		}

		public void setPacketNumber(int number)
		{
			this.packetNumber = number;
		}

		public override bool Equals(object packet)
		{
			if (!(packet is SoePacket))
			{
				return false;
			}
			SoePacket soepacket = (SoePacket)packet;
			bool result = false;
			if (this.Data.Count == soepacket.Data.Count)
			{
				for (int i = 0; i < this.Data.Count; i++)
				{
					if (soepacket.Data[i] != this.Data[i])
					{
						break;
					}
				}
				result = true;
			}
			return result;
		}

		protected string name;
		protected int packetNumber;
		protected UdpPacket udpPacket;
		protected Packet rawPacket;
		protected IPAddress sourceAddress;
		protected int sourcePort;
		protected IPAddress destinationAddress;
		protected int destinationPort;
		protected PacketOrigin packetOrigin;
		protected string serverOrigin;
		protected PosixTimeval time;
		protected ushort SoeOpcode;
		private bool Encrypted;
		private bool Compressed;
		protected ushort PacketCrc;
		public List<SoePacket> SOEPacketList = new List<SoePacket>();
		public List<SwgPacket> SWGPackets = new List<SwgPacket>();
		private List<string> PacketDisplay;
		protected bool hasFooter;
		protected uint sequenceNumber;
		protected bool HasMultipleInternalPackets;
		public IPacketNode MyNode;
		protected bool doProcess = true;
	}
}
