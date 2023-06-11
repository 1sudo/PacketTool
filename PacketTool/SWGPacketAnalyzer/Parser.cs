using System;
using System.Collections.Generic;

namespace SwgPacketAnalyzer;

public class Parser
{
	internal byte ParseByte(int offset = 0)
	{
		return this.Data[offset];
	}

	internal ushort ParseNetByteShort(int offset = 0)
	{
		ushort num = 0;
		int num2 = 1;
		int num3 = 0;
		do
		{
			byte b = this.Data[offset + num2];
			num += (ushort)(b << num3 * 8);
			num3++;
			num2--;
		}
		while (num2 >= 0);
		return num;
	}

	internal ushort ParseShort(int offset = 0)
	{
		ushort num = 0;
		for (int i = 0; i < 2; i++)
		{
			byte b = this.Data[i + offset];
			num += (ushort)(b << i * 8);
		}
		return num;
	}

	internal uint ParseInt(int offset = 0)
	{
		uint num = 0U;
		for (int i = 0; i < 4; i++)
		{
			byte b = this.Data[i + offset];
			num += (uint)((uint)b << i * 8);
		}
		return num;
	}

	internal uint ParseNetByteInt(int offset = 0)
	{
		uint num = 0U;
		int num2 = 3;
		int num3 = 0;
		do
		{
			byte b = this.Data[num2 + offset];
			num += (uint)((uint)b << num3 * 8);
			num3++;
			num2--;
		}
		while (num2 >= 0);
		return num;
	}

	internal ulong ParseLong(int offset = 0)
	{
		ulong num = 0UL;
		for (int i = 0; i < 8; i++)
		{
			byte b = this.Data[i + offset];
			num += (ulong)((long)((long)b << i * 8));
		}
		return num;
	}

	internal ulong ParseNetByteLong(int offset = 0)
	{
		ulong num = 0UL;
		int num2 = 7;
		int num3 = 0;
		do
		{
			byte b = this.Data[num2 + offset];
			num += (ulong)((long)((long)b << num3 * 8));
			num3++;
			num2--;
		}
		while (num2 >= 0);
		return num;
	}

	internal int GetRawLineCount(List<byte> inData)
	{
		if (inData.Count % 16 <= 0)
		{
			return inData.Count / 16;
		}
		return inData.Count / 16 + 1;
	}

	internal int GetLineByteCount(List<byte> inData, int i)
	{
		if ((i + 1) * 16 <= inData.Count)
		{
			return 16;
		}
		return inData.Count - i * 16;
	}

	internal string[] GenerateRawView(List<byte> inData)
	{
		List<string> list = new List<string>();
		int rawLineCount = this.GetRawLineCount(inData);
		for (int i = 0; i < rawLineCount; i++)
		{
			string text = (i * 16).ToString("X4") + ":   ";
			int num = ((i + 1) * 16 > inData.Count) ? (inData.Count - i * 16) : 16;
			for (int j = 0; j < num; j++)
			{
				text = text + inData[i * 16 + j].ToString("X2") + " ";
			}
			for (int k = num; k < 16; k++)
			{
				text += "   ";
			}
			text += "   ";
			for (int l = 0; l < num; l++)
			{
				char c = Convert.ToChar(inData[i * 16 + l]);
				if (c < '!' || c > '~')
				{
					c = '.';
				}
				text += c;
			}
			list.Add(text);
		}
		list.Add("");
		return list.ToArray();
	}

	internal virtual string[] GetPacketBreakdown(bool update = false)
	{
		List<string> list = new List<string>();
		return list.ToArray();
	}

	public List<byte> Data = new List<byte>();
}
