using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Controls;
using SwgPacketAnalyzer.SwgPackets;

namespace SwgPacketAnalyzer.nodes
{
	public class SoePacketTreeNode : TreeViewItem, IPacketNode
	{
		public List<SwgPacketTreeNode> Children { get; } = new();
		
		public SoePacketTreeNode(SoePacket soePacket)
		{
			Trace.WriteLine("packet 1");
			this.packet = soePacket;
			string text;
			if (soePacket.getPacketOrigin() == PacketOrigin.Client)
			{
                Trace.WriteLine("packet 2");
                text = "C->S";
			}
			else
			{
                Trace.WriteLine("packet 3");
                text = "S->C";
			}
            Trace.WriteLine("packet 4");
            string arg = soePacket.getPacketNumber() + ". " + this.packet.getName();
            Trace.WriteLine("packet 6");
            string arg2 = string.Concat(new string[]
			{
				"( ",
				this.packet.getServerOrigin(),
				": ",
				text,
				" )"
			});
            Trace.WriteLine("packet 7");
            base.Header = string.Format("{0, -27} {1, 13}", arg, arg2);
            Trace.WriteLine("packet 8");
            this.packet.MyNode = this;
            Trace.WriteLine("packet 9");
            this.CreateChildNodes();
            Trace.WriteLine("packet 10");
        }

		private void CreateChildNodes()
		{
			foreach (SwgPacket swgPacket in packet.SWGPackets)
			{
				SwgPacketTreeNode swgPacketTreeNode = new(swgPacket);
				swgPacket.MyNode = swgPacketTreeNode;
				Children.Add(swgPacketTreeNode);
			}
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
