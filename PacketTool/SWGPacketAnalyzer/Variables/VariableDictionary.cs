using System;
using System.Collections.Generic;

namespace SwgPacketAnalyzer.variables
{
	public class VariableDictionary : List<Variable>
	{
		public VariableDictionary(uint crc, string name, long lastupdate)
		{
			this.PacketCRC = crc;
			this.Name = name;
			this.LastUpdate = lastupdate;
		}
		
		private uint PacketCRC;
		private string Name;
		private long LastUpdate;
	}
}
