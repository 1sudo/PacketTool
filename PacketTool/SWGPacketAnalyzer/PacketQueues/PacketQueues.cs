using System.Collections.Generic;
using System.Windows;
using SwgPacketAnalyzer.SwgPackets;

namespace SwgPacketAnalyzer.packetqueues
{
	internal class PacketQueues
	{
		public PacketQueues()
		{
			this.queues.Add("Login", new PacketQueue("Login"));
			this.queues.Add("Zone", new PacketQueue("Zone"));
		}

		internal void ClearAll()
		{
			foreach (PacketQueue packetQueue in this.queues.Values)
			{
				packetQueue.ClearAll();
			}
		}

		internal void addPacket(SoePacket spacket)
		{
			if (spacket.getServerOrigin() == "Ping")
			{
				return;
			}
			if (this.queues.ContainsKey(spacket.getServerOrigin()))
			{
				PacketQueue packetQueue = this.queues[spacket.getServerOrigin()];
				packetQueue.processPacket(spacket);
				return;
			}

            MessageBox.Show("Error!", 
				"Got a packet from Origin: " + spacket.getServerOrigin() + " that doesn't have a handler");
		}

		internal IEnumerable<PacketQueue> getQueues()
		{
			return this.queues.Values;
		}

		internal void remove(SoePacket soePacket)
		{
			foreach (PacketQueue packetQueue in this.queues.Values)
			{
				packetQueue.removePackets(soePacket);
			}
		}

		private Dictionary<string, PacketQueue> queues = new();
	}
}
