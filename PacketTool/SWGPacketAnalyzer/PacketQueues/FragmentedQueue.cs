using System;
using System.Collections.Generic;
using System.Windows;
using SwgPacketAnalyzer.SwgPackets;

namespace SwgPacketAnalyzer.packetqueues
{
	internal class FragmentedQueue : SortedList<uint, FragmentedPacket>
	{
		internal void addPacket(FragmentedPacket fragmentedPacket)
		{
			try
			{
				if (fragmentedPacket.isInitial() && this.getInitialFragment() != null)
				{
					base.Clear();
				}
				if (!base.ContainsKey(fragmentedPacket.getSequenceNumber()))
				{
					base.Add(fragmentedPacket.getSequenceNumber(), fragmentedPacket);
				}
			}
			catch (Exception ex)
			{
				if (!base[fragmentedPacket.getSequenceNumber()].Equals(fragmentedPacket))
				{
                    MessageBox.Show("Error!", "FragmentedQueue::addPacket duplicate sequence number. " + ex.Message);
				}
			}
		}

		internal SoePacket getCompletedPacket()
		{
			if (!this.hasCompletePacket())
			{
				return null;
			}
			FragmentedPacket initialFragment = this.getInitialFragment();
			int neededCount = initialFragment.getNeededCount();
			List<uint> list = new List<uint>();
			uint num = initialFragment.getSequenceNumber() + 1U;
			while ((ulong)num < (ulong)initialFragment.getSequenceNumber() + (ulong)((long)neededCount))
			{
				FragmentedPacket fragmentedPacket = base[num];
				list.Add(num);
				initialFragment.AddFragment(fragmentedPacket);
				num += 1U;
			}
			if (!initialFragment.isComplete())
			{
				MessageBox.Show("Error!",
					"There must be a problem with the fragmentation checking algorithm, you should never see this");
				return null;
			}
			foreach (uint key in list)
			{
				base.Remove(key);
			}
			base.Remove(initialFragment.getSequenceNumber());
			initialFragment.BreakdownPacket();
			return initialFragment;
		}

		internal bool hasCompletePacket()
		{
			FragmentedPacket initialFragment = this.getInitialFragment();
			if (initialFragment == null)
			{
				return false;
			}
			int neededCount = initialFragment.getNeededCount();
			uint num = initialFragment.getSequenceNumber();
			while ((ulong)num < (ulong)initialFragment.getSequenceNumber() + (ulong)((long)neededCount))
			{
				if (!base.ContainsKey(num))
				{
					return false;
				}
				num += 1U;
			}
			return true;
		}

		private FragmentedPacket getInitialFragment()
		{
			for (int i = 0; i < base.Count; i++)
			{
				FragmentedPacket fragmentedPacket = base[base.Keys[i]];
				if (fragmentedPacket.isInitial())
				{
					return fragmentedPacket;
				}
			}
			return null;
		}
	}
}
