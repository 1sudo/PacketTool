using System.Windows.Controls;
using SwgPacketAnalyzer.SwgPackets;

namespace SwgPacketAnalyzer.nodes
{
	public class ErrorPacketTreeNode : TreeViewItem, IPacketNode
	{
		public ErrorPacketTreeNode(ErrorPacket p)
		{
			this.packet = p;
			base.Header = "Error";
		}

		public string[] GetDisplayView()
		{
			return this.packet.GetDisplayView();
		}

		public string[] GetPacketBreakdown()
		{
			return this.packet.GetPacketBreakdown(false);
		}

		private ErrorPacket packet;
	}
}
