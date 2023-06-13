using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Input;
using PacketTool.Models;
using PacketTool.Views;
using SwgPacketAnalyzer;
using Color = System.Drawing.Color;

namespace PacketTool.ViewModels;

public enum ContentPosition
{
    Start = 0,
    End = 1
}

public class PacketEditorViewModel : ViewModelBase
{
	public RelayCommand? ByteButton { get; }
	public RelayCommand? AsciiButton { get; }
	public RelayCommand? ShortButton { get; }
	public RelayCommand? UnicodeButton { get; }
	public RelayCommand? IntButton { get; }
	public RelayCommand? FloatButton { get; }
	public RelayCommand? LongButton { get; }
	public RelayCommand? CrcButton { get; }
	public RelayCommand? ListButton { get; }
    private SwgPacket Packet { get; set; }
    private SwgPacket ActivePacket { get; set; } 
    public static Action<RichTextBox, RichTextBoxType>? OnRichTextBox { get; set; }
    private RichTextBox LineNumberRichTextBox { get; set; }
    private RichTextBox HexEditorRichTextBox { get; set; }
    private RichTextBox AsciiEditorRichTextBox { get; set; }
    private RichTextBox BreakdownRichTextBox { get; set; }
    public static int HexLineLength = 48;
    public static int AsciiLineLength = 17;
    private int nextOffset;
    private bool listenabled;
    private int listid;
    private int listindex;
    private bool blockSelectedIndexUpdate;

    public PacketEditorViewModel(SwgPacket packet)
    {
        OnRichTextBox += SetRichTextBox;
        Packet = packet;
        ActivePacket = new SwgPacket(packet, 0);
        UpdateWindow(packet);

        ByteButton = new RelayCommand(AddByte);
        AsciiButton = new RelayCommand(addAscii);
	    ShortButton = new RelayCommand(AddShort);
        UnicodeButton = new RelayCommand(addUnicode);
	    IntButton = new RelayCommand(AddInt);
        FloatButton = new RelayCommand(AddFloat); 
	    LongButton = new RelayCommand(AddLong);
        CrcButton = new RelayCommand(addCRC);
        ListButton = new RelayCommand(ListButtonClicked);
    }

    void SetRichTextBox(RichTextBox textBox, RichTextBoxType type)
    {
        switch (type)
        {
            case RichTextBoxType.LineNumber: 
                LineNumberRichTextBox = textBox;
                break;
            case RichTextBoxType.HexEditor:
                HexEditorRichTextBox = textBox;
                break;
            case RichTextBoxType.AsciiEditor:
                AsciiEditorRichTextBox = textBox;
                break;
            case RichTextBoxType.Breakdown:
                BreakdownRichTextBox = textBox;
                break;
        }
    }

    private void UpdateWindow(SwgPacket packet)
    {
        LineNumbersTextBox = "";
        HexEditorTextBox = "";
        AsciiEditorTextBox = "";
        BreakdownTextBox = "";
        BreakdownListBox.Clear();
        LineNumbersTextBox = GetStringFromLines(packet.GetLineNumbersForEditor());
        HexEditorTextBox = GetStringFromLines(packet.GetHexForEditor());
        AsciiEditorTextBox = GetStringFromLines(packet.GetAsciiForEditor());
        BreakdownTextBox = GetStringFromLines(packet.GetPacketBreakdown());
        nextOffset = this.ApplyHighlightsAndBuildStructure(packet);
        PreviewVariables(nextOffset);
        HexEditorRichTextBox.CaretPosition = HexEditorRichTextBox.CaretPosition.DocumentStart;
        AsciiEditorRichTextBox.CaretPosition = AsciiEditorRichTextBox.CaretPosition.DocumentStart;
    }
    
    private int ApplyHighlightsAndBuildStructure(SwgPacket packetstruct)
    {
        int num = 0;
        int num2 = 0;
        int num3 = -1;
        bool flag = false;
        bool flag2 = false;
        ArrayList arrayList = new ArrayList();
        foreach (Variable variable in packetstruct.myStruct)
        {
            variable.index = num2;
            num2++;
            if (!this.listenabled)
            {
                if (num3 != variable.listid)
                {
                    flag2 = (num3 != -1 && variable.listid > num3);
                    if (variable.listindex != -1)
                    {
                        flag = true;
                        num3 = variable.listid;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                if (flag2)
                {
                    int num4 = int.Parse(packetstruct.myStruct[num3].currentvalue);
                    if (!flag)
                    {
                        num3 = -1;
                    }
                    for (int i = 0; i < num4; i++)
                    {
                        foreach (object obj in arrayList)
                        {
                            Variable myvar = (Variable)obj;
                            this.HighlightPacket(packetstruct, myvar, num, i == 0);
                            num += variable.getLength();
                        }
                    }
                    arrayList.Clear();
                }
                if (flag)
                {
                    arrayList.Add(variable);
                    continue;
                }
            }
            this.HighlightPacket(packetstruct, variable, num, true);
            num += variable.getLength();
        }
        if (arrayList.Count > 0)
        {
            int num5 = int.Parse(packetstruct.myStruct[num3].currentvalue);
            for (int j = 0; j < num5; j++)
            {
                foreach (object obj2 in arrayList)
                {
                    Variable variable2 = (Variable)obj2;
                    this.HighlightPacket(packetstruct, variable2, num, j == 0);
                    num += variable2.getLength();
                    num2++;
                }
            }
            arrayList.Clear();
        }
        return num;
    }
    
    private void HighlightPacket(SwgPacket packetstruct, Variable myvar, int currentOffset, bool add = true)
    {
        string text = myvar.getName();
        if (!myvar.isComplete())
        {
            text += "(Incomplete)";
        }
        if (add)
        {
            BreakdownListBox.Add(text);
        }
        string text2 = "";
        if (myvar.getType() == VariableType.Ascii || myvar.getType() == VariableType.Unicode)
        {
            int num = 1;
            if (myvar.getType() == VariableType.Ascii)
            {
                string rangeAsString = packetstruct.GetRangeAsString(currentOffset, 2);
                text2 = packetstruct.ConvertBytesToValueString(VariableType.Short, ByteOrder.HostByte, myvar.isHex(), rangeAsString);
            }
            else
            {
                string rangeAsString = packetstruct.GetRangeAsString(currentOffset, 4);
                text2 = packetstruct.ConvertBytesToValueString(VariableType.Int, ByteOrder.HostByte, myvar.isHex(), rangeAsString);
                num = 2;
            }
            try
            {
                myvar.setStringLength(int.Parse(text2) * num);
            }
            catch
            {
            }
        }
        try
        {
            text2 = packetstruct.ConvertBytesToValueString(myvar.getType(), myvar.getByteOrder(), myvar.isHex(), packetstruct.GetRangeAsString(currentOffset, myvar.getLength()));
        }
        catch
        {
        }
        Color color;
        if (myvar.isComplete())
        {
            color = Color.LightGreen;
        }
        else
        {
            color = Color.LightGray;
        }
        if (myvar.index == BreakdownSelectedIndex)
        {
            color = Color.Yellow;
        }
        myvar.currentvalue = text2;
        this.HighlightText(currentOffset, myvar.getLength(), color);
    }
    
    private void HighlightText(int offset, int length, Color color)
    {
        int asciiLocationFromDataOffset = this.GetAsciiLocationFromDataOffset(offset);
        int asciiLengthFromDataOffset = this.GetAsciiLengthFromDataOffset(offset, length);
        
        AsciiEditorRichTextBox.Selection.Select(
            GetPosition(AsciiEditorRichTextBox, ContentPosition.Start, asciiLocationFromDataOffset), 
            GetPosition(AsciiEditorRichTextBox, ContentPosition.End, asciiLengthFromDataOffset));

        AsciiEditorRichTextBox.SelectionBrush = ConvertColor(color);
        
        int hexLocationFromDataOffset = this.GetHexLocationFromDataOffset(offset);
        int hexLengthFromDataOffset = this.GetHexLengthFromDataOffset(offset, length);
        
        HexEditorRichTextBox.Selection.Select(
            GetPosition(HexEditorRichTextBox, ContentPosition.Start, hexLocationFromDataOffset), 
            GetPosition(HexEditorRichTextBox, ContentPosition.End, hexLengthFromDataOffset));

        HexEditorRichTextBox.SelectionBrush = ConvertColor(color);

        HexEditorRichTextBox.Selection.Select(
            GetPosition(HexEditorRichTextBox, ContentPosition.Start, 0), 
            GetPosition(HexEditorRichTextBox, ContentPosition.End, 0));
        
        AsciiEditorRichTextBox.Selection.Select(
            GetPosition(AsciiEditorRichTextBox, ContentPosition.Start, 0), 
            GetPosition(AsciiEditorRichTextBox, ContentPosition.End, 0));
    }
    
    private int GetAsciiLocation(int offset)
    {
        int num = offset / HexLineLength;
        int num2 = offset % HexLineLength;
        return num * 17 + num2 / 3;
    }

    private int GetAsciiLength(int offset, int length)
    {
        int asciiLocation = this.GetAsciiLocation(offset);
        int asciiLocation2 = this.GetAsciiLocation(offset + length);
        return asciiLocation2 - asciiLocation;
    }

    private int GetDataOffsetFromHexOffset(int hexoffset)
    {
        int num = hexoffset / HexLineLength;
        int num2 = hexoffset % HexLineLength;
        return num * 16 + num2 / 3;
    }

    private int GetHexLocationFromDataOffset(int offset)
    {
        int num = offset * 3;
        return num + num / HexLineLength;
    }

    private int GetHexLengthFromDataOffset(int offset, int length)
    {
        int hexLocationFromDataOffset = this.GetHexLocationFromDataOffset(offset);
        int hexLocationFromDataOffset2 = this.GetHexLocationFromDataOffset(offset + length);
        return hexLocationFromDataOffset2 - hexLocationFromDataOffset;
    }

    private int GetAsciiLocationFromDataOffset(int offset)
    {
        return offset + offset / AsciiLineLength;
    }

    private int GetAsciiLengthFromDataOffset(int offset, int length)
    {
        int asciiLocationFromDataOffset = this.GetAsciiLocationFromDataOffset(offset);
        int asciiLocationFromDataOffset2 = this.GetAsciiLocationFromDataOffset(offset + length);
        return asciiLocationFromDataOffset2 - asciiLocationFromDataOffset;
    }

    private SolidColorBrush ConvertColor(System.Drawing.Color color)
    {
        return new SolidColorBrush(new System.Windows.Media.Color
        {
            A = color.A,
            R = color.R,
            G = color.G,
            B = color.B
        });
    }

    TextPointer GetPosition(RichTextBox textbox, ContentPosition position, int offset)
    {
        switch (position)
        {
            case ContentPosition.Start:
            {
                TextPointer? pointer = textbox.Document.ContentStart.GetPositionAtOffset(offset);
                if (pointer is not null) return pointer;
                break;
            }
            case ContentPosition.End:
            {
                TextPointer? pointer = textbox.Document.ContentEnd.GetPositionAtOffset(offset);
                if (pointer is not null) return pointer;
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(position), position, null);
        }
        
        throw new ArgumentOutOfRangeException(nameof(position), position, null);
    }
    
    private void enableButtons()
    {
	    ByteButtonEnabled = true;
	    ByteMenuStripEnabled = true;
	    IntMenuStripEnabled = true;
	    IntButtonEnabled = true;
	    ShortMenuStripEnabled = true;
	    ShortButtonEnabled = true;
	    FloatButtonEnabled = true;
	    FloatMenuStripEnabled = true;
	    LongButtonEnabled = true;
	    LongMenuStripEnabled = true;
	    AsciiButtonEnabled = true;
	    AsciiMenuStripEnabled = true;
	    UnicodeButtonEnabled = true;
	    UnicodeMenuStripEnabled = true;
    }
    
    private void clearText()
    {
	    ByteTextBox = "N/A";
	    ShortTextBox = "N/A";
	    IntTextBox = "N/A";
	    LongTextBox = "N/A";
	    FloatTextBox = "N/A";
	    AsciiTextBox = "N/A";
	    UnicodeTextBox = "N/A";
    }

    private string GetAllText(RichTextBox textbox)
    {
        return new TextRange(textbox.Document.ContentStart, textbox.Document.ContentEnd).Text;
    }
    
    private void PreviewVariables(int offset)
    {
        this.enableButtons();
        this.clearText();
        try
        {
            string hexEditorText = GetAllText(HexEditorRichTextBox);
            string text = hexEditorText.Trim().Replace("\n", "").Substring(offset * 3);
            if (!string.IsNullOrEmpty(text))
            {
                string[] array = text.Split(new char[]
                {
                    ' '
                });
                List<byte> list = new List<byte>();
                foreach (string text2 in array)
                {
                    list.Add(Convert.ToByte(text2.Replace("\r", ""), 16));
                }
                try
                {
                    this.BytePreview(list.GetRange(0, 1));
                }
                catch
                {
                }
                try
                {
                    this.ShortPreview(list.GetRange(0, 2));
                }
                catch
                {
                }
                try
                {
                    this.IntPreview(list.GetRange(0, 4));
                }
                catch
                {
                }
                try
                {
                    this.FloatPreview(list.GetRange(0, 4));
                }
                catch
                {
                }
                try
                {
                    this.LongPreview(list.GetRange(0, 8));
                }
                catch
                {
                }
                try
                {
                    this.CrcPreview(list.GetRange(0, 4));
                }
                catch
                {
                }
                int num = (int)this.getShort(list.GetRange(0, 2));
                if (num > 0 && num <= list.Count - 2)
                {
                    this.AsciiPreview(list.GetRange(2, num));
                }
                else
                {
	                AsciiButtonEnabled = false;
	                AsciiMenuStripEnabled = false;
                }
                num = (int)this.getInt(list.GetRange(0, 4));
                if (num > 0 && num <= list.Count - 4)
                {
                    this.UnicodePreview(list.GetRange(4, num * 2));
                }
                else
                {
	                UnicodeButtonEnabled = false;
	                UnicodeMenuStripEnabled = false;
                }
            }
        }
        catch
        {
        }
    }

    private string GetStringFromLines(string[] lines)
    {
        StringBuilder sb = new();
        
        foreach (string line in lines)
        {
            sb.AppendLine(line);
        }

        return sb.ToString();
    }
    
    private void UnicodePreview(List<byte> bytes)
	{
		if (bytes.Count >= 4)
		{
			UnicodeButtonEnabled = true;
			UnicodeMenuStripEnabled = true;
			string text = "";
			for (int i = 0; i < bytes.Count; i += 2)
			{
				char c = Convert.ToChar(bytes[i]);
				if (this.isValidChar(c))
				{
					text += c;
				}
			}
			UnicodeTextBox = text;
		}
	}

	private void AsciiPreview(List<byte> bytes)
	{
		if (bytes.Count >= 2)
		{
			AsciiButtonEnabled = true;
			AsciiMenuStripEnabled = true;
			string text = "";
			for (int i = 0; i < bytes.Count; i++)
			{
				char c = Convert.ToChar(bytes[i]);
				if (this.isValidChar(c))
				{
					text += c;
				}
			}
			AsciiTextBox = text;
		}
	}

	private void CrcPreview(List<byte> bytes)
	{
		if (bytes.Count >= 4)
		{
			string text = MainWindowModel.CrcLookup(this.getInt(bytes));
			if (!string.IsNullOrEmpty(text))
			{
				CrcButtonEnabled = true;
				CrcTextBox = text;
				return;
			}

			CrcButtonEnabled = false;
			CrcTextBox = "N/A";
		}
	}

	private bool isValidChar(char val)
	{
		return val >= ' ' && val <= '\u007f';
	}

	private void LongPreview(List<byte> bytes)
	{
		if (bytes.Count >= 8)
		{
			LongTextBox = BitConverter.ToUInt64(bytes.ToArray(), 0).ToString();
			LongButtonEnabled = true;
			LongMenuStripEnabled = true;
		}
	}

	private void FloatPreview(List<byte> bytes)
	{
		if (bytes.Count >= 4)
		{
			FloatTextBox = BitConverter.ToSingle(bytes.ToArray(), 0).ToString();
			FloatButtonEnabled = true;
			FloatMenuStripEnabled = true;
		}
	}

	private void BytePreview(List<byte> bytes)
	{
		if (bytes.Count >= 1)
		{
			ByteTextBox = bytes[0].ToString();
			ByteButtonEnabled = true;
			ByteMenuStripEnabled = true;
		}
	}

	private void byteButton_Click(object sender, EventArgs e)
	{
		this.AddByte();
	}

	private void byteToolStripMenuItem_Click(object sender, EventArgs e)
	{
		this.AddByte();
	}

	private void AddByte()
	{
		this.addVariable(this.nextOffset, 1, VariableType.Byte, ByteOrder.HostByte, "");
	}

	private void shortButton_Click(object sender, EventArgs e)
	{
		this.AddShort();
	}

	private void shortToolStripMenuItem_Click(object sender, EventArgs e)
	{
		this.AddShort();
	}

	private void AddShort()
	{
		this.addVariable(this.nextOffset, 2, VariableType.Short, ByteOrder.HostByte, "");
	}

	private void ShortPreview(List<byte> bytes)
	{
		if (bytes.Count >= 2)
		{
			ShortTextBox = getShort(bytes).ToString();
			ShortButtonEnabled = true;
			ShortMenuStripEnabled = true;
		}
	}

	private ushort getShort(List<byte> bytes)
	{
		return BitConverter.ToUInt16(bytes.ToArray(), 0);
	}

	private void intButton_Click(object sender, EventArgs e)
	{
		this.AddInt();
	}

	private void intToolStripMenuItem_Click(object sender, EventArgs e)
	{
		this.AddInt();
	}

	private void AddInt()
	{
		this.addVariable(this.nextOffset, 4, VariableType.Int, ByteOrder.HostByte, "");
	}

	private void IntPreview(List<byte> bytes)
	{
		if (bytes.Count >= 4)
		{
			IntTextBox = getInt(bytes).ToString();
			IntButtonEnabled = true;
			IntMenuStripEnabled = true;
		}
	}

	private uint getInt(List<byte> bytes)
	{
		return BitConverter.ToUInt32(bytes.ToArray(), 0);
	}

	private void floatButton_Click(object sender, EventArgs e)
	{
		this.AddFloat();
	}

	private void floatToolStripMenuItem_Click(object sender, EventArgs e)
	{
		this.AddFloat();
	}

	private void AddFloat()
	{
		this.addVariable(this.nextOffset, 4, VariableType.Float, ByteOrder.HostByte, "");
	}

	private void AddLong()
	{
		this.addVariable(this.nextOffset, 8, VariableType.Long, ByteOrder.HostByte, "");
	}

	private void addAscii()
	{
		this.addVariable(this.nextOffset, 2, VariableType.Ascii, ByteOrder.HostByte, "");
	}

	private void unicodeToolStripMenuItem_Click(object sender, EventArgs e)
	{
		this.addUnicode();
	}

	private void addUnicode()
	{
		this.addVariable(this.nextOffset, 4, VariableType.Unicode, ByteOrder.HostByte, "");
	}

	private void addCRC()
	{
		this.addVariable(this.nextOffset, 4, VariableType.Int, ByteOrder.HostByte, "");
	}

	private void addVariable(int offset, int length, VariableType variableType, ByteOrder byteOrder, string description = "")
	{
		if (description.Equals("n/a"))
		{
			description = "";
		}
		VariableEditor variableEditor = new VariableEditor(offset, length, variableType, byteOrder, description);
		
		bool? dialogResult = variableEditor.ShowDialog();
		if (dialogResult == true && variableEditor.MyVariable != null && ActivePacket != null)
		{
			try
			{
				if (this.listenabled)
				{
					variableEditor.MyVariable.listindex = this.listindex;
					this.listindex++;
					variableEditor.MyVariable.listid = this.listid;
				}
				ActivePacket.myStruct.Add(variableEditor.MyVariable);
				UpdateWindow(ActivePacket);
			}
			catch
			{
			}
		}
	}

	void ListButtonClicked()
	{
		if (listenabled)
		{
			listenabled = false;
			ListButtonText = "Start List";
			listid = -1;
			return;
		}
		if (BreakdownSelectedIndex == -1)
		{
			MessageBox.Show("Select an item to be the list counter from the right");
			return;
		}
		this.listenabled = true;
		ListButtonText = "Stop List";
		this.listid = ActivePacket.myStruct[BreakdownSelectedIndex].index;
		this.listindex = 0;
		int selected = BreakdownSelectedIndex;
		string info = ActivePacket.myStruct[selected].currentvalue.ToString();
		// comboBox1.Text = info;
		UpdateWindow(ActivePacket);
	}

	void OnSelectedIndexChanged()
	{
		if (blockSelectedIndexUpdate) return;
		
		blockSelectedIndexUpdate = true;
		int selectedIndex = BreakdownSelectedIndex;
		UpdateWindow(ActivePacket);
		BreakdownSelectedIndex = selectedIndex;
		int num = 0;
		int num2 = 0;
		foreach (Variable variable in ActivePacket.myStruct)
		{
			if (variable.getType() == VariableType.Ascii || variable.getType() == VariableType.Unicode)
			{
				int num3 = 1;
				string s;
				if (variable.getType() == VariableType.Ascii)
				{
					string rangeAsString = ActivePacket.GetRangeAsString(num, 2);
					s = ActivePacket.ConvertBytesToValueString(VariableType.Short, ByteOrder.HostByte, variable.isHex(), rangeAsString);
				}
				else
				{
					string rangeAsString = ActivePacket.GetRangeAsString(num, 4);
					s = ActivePacket.ConvertBytesToValueString(VariableType.Int, ByteOrder.HostByte, variable.isHex(), rangeAsString);
					num3 = 2;
				}
				try
				{
					variable.setStringLength(int.Parse(s) * num3);
				}
				catch
				{
				}
			}
			if (variable.index == BreakdownSelectedIndex)
			{
				Color yellow = Color.Yellow;
				this.HighlightText(num, variable.getLength(), yellow);
				this.blockSelectedIndexUpdate = false;
				return;
			}
			num += variable.getLength();
			num2++;
		}
		this.blockSelectedIndexUpdate = false;
	}
    
    private string _lineNumbersTextBox;
    private string _hexEditorTextbox;
    private string _asciiEditorTextBox;
    private string _breakdownTextBox;
    private ObservableCollection<string> _breakdownListBox = new();
    private int _breakdownSelectedIndex;
    private bool _byteMenuStripEnabled;
    private bool _shortMenuStripEnabled;
    private bool _intMenuStripEnabled;
    private bool _floatMenuStripEnabled;
    private bool _longMenuStripEnabled;
    private bool _asciiMenuStripEnabled;
    private bool _unicodeMenuStripEnabled;
    private bool _byteButtonEnabled;
    private bool _asciiButtonEnabled;
    private bool _shortButtonEnabled;
    private bool _unicodeButtonEnabled;
    private bool _intButtonEnabled;
    private bool _floatButtonEnabled;
    private bool _longButtonEnabled;
    private bool _crcButtonEnabled;
    private string _byteTextBox;
    private string _asciiTextBox;
    private string _shortTextBox;
    private string _unicodeTextBox;
    private string _intTextBox;
    private string _floatTextBox;
    private string _longTextBox;
    private string _crcTextBox;
    private string _listButtonText = "Start List";
    
    public string LineNumbersTextBox
    {
        get => _lineNumbersTextBox;
        set => SetProperty(ref _lineNumbersTextBox, value);
    }

    public string HexEditorTextBox
    {
        get => _hexEditorTextbox;
        set => SetProperty(ref _hexEditorTextbox, value);
    }

    public string AsciiEditorTextBox
    {
        get => _asciiEditorTextBox;
        set => SetProperty(ref _asciiEditorTextBox, value);
    }

    public string BreakdownTextBox
    {
        get => _breakdownTextBox;
        set => SetProperty(ref _breakdownTextBox, value);
    }

    public ObservableCollection<string> BreakdownListBox
    {
        get => _breakdownListBox;
        set => SetProperty(ref _breakdownListBox, value);
    }

    public int BreakdownSelectedIndex
    {
        get => _breakdownSelectedIndex;
        set
        {
	        SetProperty(ref _breakdownSelectedIndex, value);
	        OnSelectedIndexChanged();
        } 
	        
    }

    public bool ByteMenuStripEnabled
    {
	    get => _byteMenuStripEnabled;
	    set => SetProperty(ref _byteButtonEnabled, value);
    }
    
    public bool ShortMenuStripEnabled
    {
	    get => _shortMenuStripEnabled;
	    set => SetProperty(ref _shortMenuStripEnabled, value);
    }
    
    public bool IntMenuStripEnabled
    {
	    get => _intMenuStripEnabled;
	    set => SetProperty(ref _intMenuStripEnabled, value);
    }
    
    public bool FloatMenuStripEnabled
    {
	    get => _floatMenuStripEnabled;
	    set => SetProperty(ref _floatMenuStripEnabled, value);
    }
    
    public bool LongMenuStripEnabled
    {
	    get => _longMenuStripEnabled;
	    set => SetProperty(ref _longMenuStripEnabled, value);
    }
    
    public bool AsciiMenuStripEnabled
    {
	    get => _asciiMenuStripEnabled;
	    set => SetProperty(ref _asciiMenuStripEnabled, value);
    }
    
    public bool UnicodeMenuStripEnabled
    {
	    get => _unicodeMenuStripEnabled;
	    set => SetProperty(ref _unicodeMenuStripEnabled, value);
    }

    public bool ByteButtonEnabled
    {
	    get => _byteButtonEnabled;
	    set => SetProperty(ref _byteButtonEnabled, value);
    }
    
    public bool AsciiButtonEnabled
    {
	    get => _asciiButtonEnabled;
	    set => SetProperty(ref _asciiButtonEnabled, value);
    }
    
    public bool ShortButtonEnabled
    {
	    get => _shortButtonEnabled;
	    set => SetProperty(ref _shortButtonEnabled, value);
    }
    
    public bool UnicodeButtonEnabled
    {
	    get => _unicodeButtonEnabled;
	    set => SetProperty(ref _unicodeButtonEnabled, value);
    }
    
    public bool IntButtonEnabled
    {
	    get => _intButtonEnabled;
	    set => SetProperty(ref _intButtonEnabled, value);
    }
    
    public bool FloatButtonEnabled
    {
	    get => _floatButtonEnabled;
	    set => SetProperty(ref _floatButtonEnabled, value);
    }
    
    public bool LongButtonEnabled
    {
	    get => _longButtonEnabled;
	    set => SetProperty(ref _longButtonEnabled, value);
    }

    public bool CrcButtonEnabled
    {
	    get => _crcButtonEnabled;
	    set => SetProperty(ref _crcButtonEnabled, value);
    }

    public string ByteTextBox
    {
	    get => _byteTextBox;
	    set => SetProperty(ref _byteTextBox, value);
    }
    
    public string AsciiTextBox
    {
	    get => _asciiTextBox;
	    set => SetProperty(ref _asciiTextBox, value);
    }
    
    public string ShortTextBox
    {
	    get => _shortTextBox;
	    set => SetProperty(ref _shortTextBox, value);
    }
    
    public string UnicodeTextBox
    {
	    get => _unicodeTextBox;
	    set => SetProperty(ref _unicodeTextBox, value);
    }
    
    public string IntTextBox
    {
	    get => _intTextBox;
	    set => SetProperty(ref _intTextBox, value);
    }
    
    public string FloatTextBox
    {
	    get => _floatTextBox;
	    set => SetProperty(ref _floatTextBox, value);
    }
    
    public string LongTextBox
    {
	    get => _longTextBox;
	    set => SetProperty(ref _longTextBox, value);
    }
    
    public string CrcTextBox
    {
	    get => _crcTextBox;
	    set => SetProperty(ref _crcTextBox, value);
    }

    public string ListButtonText
    {
	    get => _listButtonText;
	    set => SetProperty(ref _listButtonText, value);
    }
}