using System.Windows.Controls;

namespace SwgPacketAnalyzer.nodes
{
	public class SwgPacketTreeNode : TreeViewItem, IPacketNode
	{
		public SwgPacketTreeNode(SwgPacket packet, bool shortName = false)
		{
			if (!shortName && packet.mySOEPacket != null)
			{
				string text;
				if (packet.mySOEPacket.getPacketOrigin() == PacketOrigin.Client)
				{
					text = "C->S";
				}
				else
				{
					text = "S->C";
				}
				string name = packet.Name;
				string arg = string.Concat(new object[]
				{
					"(Packet ",
					packet.mySOEPacket.getPacketNumber(),
					": ",
					text,
					" )"
				});
				base.Header = string.Format("{0, -27} {1, 13}", name, arg);
			}
			else
			{
				base.Header = packet.Name;
			}
			this.gamePacket = packet;
		}

		public SwgPacket getGamePacket()
		{
			return this.gamePacket;
		}

		public string[] GetDisplayView()
		{
			return this.gamePacket.GetDisplayView();
		}

		public string[] GetPacketBreakdown()
		{
			return this.gamePacket.GetPacketBreakdown();
		}

		private SwgPacket gamePacket;
	}
}
