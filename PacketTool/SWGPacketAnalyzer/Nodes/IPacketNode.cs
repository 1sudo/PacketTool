using System;

namespace SwgPacketAnalyzer.nodes
{
	// Token: 0x02000009 RID: 9
	public interface IPacketNode
	{
		// Token: 0x06000076 RID: 118
		string[] GetDisplayView();

		// Token: 0x06000077 RID: 119
		string[] GetPacketBreakdown();
	}
}
