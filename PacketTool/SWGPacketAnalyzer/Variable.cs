using System;

namespace SwgPacketAnalyzer;

public class Variable
{
	public string getName()
	{
		return this.Name;
	}

	public int getIndex()
	{
		return this.index;
	}

	public int getLength()
	{
		return this.Length + this.stringLength;
	}

	public void setStringLength(int value)
	{
		this.stringLength = value;
	}

	public void setLength(int value)
	{
		this.Length = value;
	}

	internal VariableType getType()
	{
		return this.Type;
	}

	internal string getNotes()
	{
		return this.Notes;
	}

	internal ByteOrder getByteOrder()
	{
		return this.ByteOrder;
	}

	internal string getDescription()
	{
		return this.Description;
	}

	internal bool doShowValue()
	{
		return this.ShowValue;
	}

	internal bool isHex()
	{
		return this.ShowInHex;
	}

	internal bool isComplete()
	{
		return this.complete;
	}

	internal void setComplete(bool val)
	{
		this.complete = val;
	}

	internal int getStringLength()
	{
		return this.stringLength;
	}

	public int index;
	private int Length;
	public VariableType Type;
	public ByteOrder ByteOrder;
	public string Name;
	public string Description;
	public string Notes;
	private int stringLength;
	public bool ShowValue;
	public bool ShowInHex;
	private bool complete;
	public int listId = -1;
	public int listindex = -1;
	public string currentvalue = "";
}
