using System;
using System.Collections.Generic;
using System.Windows;
using SwgPacketAnalyzer.SwgPackets;

namespace SwgPacketAnalyzer.packetqueues
{
	internal class PacketQueue : List<SwgPacket>
	{
		public PacketQueue(string m)
		{
			this.mode = m;
			this.ClearAll();
		}

		internal void ClearAll()
		{
			base.Clear();
			this.fragmentedQueue.Clear();
		}

		internal void processPacket(SoePacket soePacket)
		{
			if (soePacket.getServerOrigin() != this.mode)
			{
                MessageBox.Show("Error!", 
					this.mode + " queue is trying to process a " + soePacket.getServerOrigin() + " packet");
				return;
			}
			try
			{
				lock (this)
				{
					if (soePacket is FragmentedPacket)
					{
						this.fragmentedQueue.addPacket((FragmentedPacket)soePacket);
						if (!this.fragmentedQueue.hasCompletePacket())
						{
							return;
						}
						soePacket = this.fragmentedQueue.getCompletedPacket();
					}
					foreach (SwgPacket item in soePacket.SWGPackets)
					{
						base.Add(item);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error!", 
					"PacketQueue::processPacket error in " + this.mode + " queue:" + ex.Message);
			}
		}

		internal void removePackets(SoePacket soePacket)
		{
			foreach (SwgPacket item in soePacket.SWGPackets)
			{
				base.Remove(item);
			}
		}

		private FragmentedQueue fragmentedQueue = new FragmentedQueue();
		private string mode = "";
	}
}
