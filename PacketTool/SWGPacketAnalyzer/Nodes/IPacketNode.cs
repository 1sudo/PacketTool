using System;

namespace SwgPacketAnalyzer.nodes
{
	public interface IPacketNode
	{
		string[] GetDisplayView();

		string[] GetPacketBreakdown();
	}
}
