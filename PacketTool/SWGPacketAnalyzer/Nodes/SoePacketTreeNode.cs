using System.Windows;
using System.Windows.Controls;
using SwgPacketAnalyzer.SwgPackets;

namespace SwgPacketAnalyzer.nodes
{
	public class SoePacketTreeNode : TreeViewItem, IPacketNode
	{	
		public SoePacketTreeNode(SoePacket soePacket)
		{
			this.packet = soePacket;
			string text;
			if (soePacket.getPacketOrigin() == PacketOrigin.Client)
			{
                text = "C->S";
			}
			else
			{
                text = "S->C";
			}
            string arg = soePacket.getPacketNumber() + ". " + this.packet.getName();
            string arg2 = string.Concat(new string[]
			{
				"( ",
				this.packet.getServerOrigin(),
				": ",
				text,
				" )"
			});
            base.Header = string.Format("{0, -27} {1, 13}", arg, arg2);
            this.packet.MyNode = this;
            this.CreateChildNodes();
        }

		private void CreateChildNodes()
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
                foreach (SwgPacket swgPacket in packet.SWGPackets)
                {
                    SwgPacketTreeNode swgPacketTreeNode = new(swgPacket);
                    swgPacket.MyNode = swgPacketTreeNode;
					Items.Add(swgPacketTreeNode);
                }
            });
		}

		public SoePacket getPacket()
		{
			return this.packet;
		}

		public string[] GetDisplayView()
		{
			return this.packet.GetDisplayView();
		}

		public string[] GetPacketBreakdown()
		{
			return this.packet.GetPacketBreakdown(false);
		}

		private SoePacket packet;
	}
}
