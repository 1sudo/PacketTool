using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
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
    // public static Action<RichTextBox, RichTextBoxType>? OnRichTextBox { get; set; }
    public static RichTextBox LineNumberRichTextBox { get; set; }
    public static RichTextBox HexEditorRichTextBox { get; set; }
    public static RichTextBox AsciiEditorRichTextBox { get; set; }
    public static RichTextBox BreakdownRichTextBox { get; set; }
    public static int HexLineLength = 48;
    public static int AsciiLineLength = 17;
    private int nextOffset;
    private bool listEnabled;
    private int listId;
    private int listIndex;
    private bool blockSelectedIndexUpdate;

    public PacketEditorViewModel(SwgPacket packet)
    {
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

    private void UpdateWindow(SwgPacket packet)
    {
        LineNumberRichTextBox.Document.Blocks.Clear();
        HexEditorRichTextBox.Document.Blocks.Clear();
        AsciiEditorRichTextBox.Document.Blocks.Clear();
        BreakdownRichTextBox.Document.Blocks.Clear();
        BreakdownListBox.Clear();
        LineNumberRichTextBox.Document.Blocks.Add(new Paragraph(new Run(GetStringFromLines(packet.GetLineNumbersForEditor()))));
        HexEditorRichTextBox.Document.Blocks.Add(new Paragraph(new Run(GetStringFromLines(packet.GetHexForEditor()))));
        AsciiEditorRichTextBox.Document.Blocks.Add(new Paragraph(new Run(GetStringFromLines(packet.GetAsciiForEditor()))));
        BreakdownRichTextBox.Document.Blocks.Add(new Paragraph(new Run(GetStringFromLines(packet.GetPacketBreakdown()))));
        nextOffset = ApplyHighlightsAndBuildStructure(packet);
        PreviewVariables(nextOffset);
        HexEditorRichTextBox.CaretPosition = HexEditorRichTextBox.CaretPosition.DocumentStart;
        AsciiEditorRichTextBox.CaretPosition = AsciiEditorRichTextBox.CaretPosition.DocumentStart;
    }
    
    private int ApplyHighlightsAndBuildStructure(SwgPacket packetStruct)
    {
        int num = 0;
        int num2 = 0;
        int num3 = -1;
        bool flag = false;
        bool flag2 = false;
        ArrayList arrayList = new();
        foreach (Variable variable in packetStruct.myStruct)
        {
            variable.index = num2;
            num2++;
            if (!listEnabled)
            {
                if (num3 != variable.listId)
                {
                    flag2 = (num3 != -1 && variable.listId > num3);
                    if (variable.listindex != -1)
                    {
                        flag = true;
                        num3 = variable.listId;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                if (flag2)
                {
                    int num4 = int.Parse(packetStruct.myStruct[num3].currentvalue);
                    if (!flag)
                    {
                        num3 = -1;
                    }
                    for (int i = 0; i < num4; i++)
                    {
                        foreach (object obj in arrayList)
                        {
                            Variable myvar = (Variable)obj;
                            this.HighlightPacket(packetStruct, myvar, num, i == 0);
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
            HighlightPacket(packetStruct, variable, num, true);
            num += variable.getLength();
        }
        if (arrayList.Count > 0)
        {
            int num5 = int.Parse(packetStruct.myStruct[num3].currentvalue);
            for (int j = 0; j < num5; j++)
            {
                foreach (object obj2 in arrayList)
                {
                    Variable variable2 = (Variable)obj2;
                    this.HighlightPacket(packetStruct, variable2, num, j == 0);
                    num += variable2.getLength();
                    num2++;
                }
            }
            arrayList.Clear();
        }
        return num;
    }
    
    private void HighlightPacket(SwgPacket packetStruct, Variable myVar, int currentOffset, bool add = true)
    {
        string text = myVar.getName();
        if (!myVar.isComplete())
        {
            text += "(Incomplete)";
        }
        if (add)
        {
            BreakdownListBox.Add(text);
        }
        string text2 = "";
        if (myVar.getType() == VariableType.Ascii || myVar.getType() == VariableType.Unicode)
        {
            int num = 1;
            if (myVar.getType() == VariableType.Ascii)
            {
                string rangeAsString = packetStruct.GetRangeAsString(currentOffset, 2);
                text2 = packetStruct.ConvertBytesToValueString(VariableType.Short, ByteOrder.HostByte, myVar.isHex(), rangeAsString);
            }
            else
            {
                string rangeAsString = packetStruct.GetRangeAsString(currentOffset, 4);
                text2 = packetStruct.ConvertBytesToValueString(VariableType.Int, ByteOrder.HostByte, myVar.isHex(), rangeAsString);
                num = 2;
            }
            try
            {
                myVar.setStringLength(int.Parse(text2) * num);
            }
            catch
            {
            }
        }
        try
        {
            text2 = packetStruct.ConvertBytesToValueString(myVar.getType(), myVar.getByteOrder(), myVar.isHex(), packetStruct.GetRangeAsString(currentOffset, myVar.getLength()));
        }
        catch { }
        Color color;
        if (myVar.isComplete())
        {
            color = Color.LightGreen;
        }
        else
        {
            color = Color.LightGray;
        }
        if (myVar.index == BreakdownSelectedIndex)
        {
            color = Color.Yellow;
        }
        myVar.currentvalue = text2;
        HighlightText(currentOffset, myVar.getLength(), color);
    }
    
    private void HighlightText(int offset, int length, Color color)
    {
        int asciiLocationFromDataOffset = this.GetAsciiLocationFromDataOffset(offset);
        int asciiLengthFromDataOffset = this.GetAsciiLengthFromDataOffset(offset, length);
        
        SetTextSelection(AsciiEditorRichTextBox, asciiLocationFromDataOffset, asciiLengthFromDataOffset);
        AsciiEditorRichTextBox.SelectionBrush = ConvertColor(color);
        
        int hexLocationFromDataOffset = this.GetHexLocationFromDataOffset(offset);
        int hexLengthFromDataOffset = this.GetHexLengthFromDataOffset(offset, length);
        
        SetTextSelection(HexEditorRichTextBox, hexLocationFromDataOffset, hexLengthFromDataOffset);
        HexEditorRichTextBox.SelectionBrush = ConvertColor(color);
        SetTextSelection(HexEditorRichTextBox, 0, 0);
        SetTextSelection(AsciiEditorRichTextBox, 0, 0);
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

    void SetTextSelection(RichTextBox textbox, int offset, int offset2)
    {
	    TextPointer myTextPointer1 = textbox.Document.ContentStart.GetPositionAtOffset(offset);  

        if (myTextPointer1 == null)
        {
            SetTextSelection(textbox, (offset - 1), offset2);
            return;
        }

	    TextPointer myTextPointer2 = textbox.Document.ContentStart.GetPositionAtOffset(offset2);

        if (myTextPointer2 == null) 
        {
            SetTextSelection(textbox, offset, (offset2 - 1));
            return;
        }

        textbox.Selection.Select(myTextPointer1, myTextPointer2);
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

	private void AddByte()
	{
		addVariable(nextOffset, 1, VariableType.Byte, ByteOrder.HostByte, "");
	}

	private void AddShort()
	{
		addVariable(this.nextOffset, 2, VariableType.Short, ByteOrder.HostByte, "");
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

	private void AddInt()
	{
		addVariable(nextOffset, 4, VariableType.Int, ByteOrder.HostByte, "");
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

	private void AddFloat()
	{
		addVariable(nextOffset, 4, VariableType.Float, ByteOrder.HostByte, "");
	}

	private void AddLong()
	{
		addVariable(nextOffset, 8, VariableType.Long, ByteOrder.HostByte, "");
	}

	private void addAscii()
	{
		addVariable(nextOffset, 2, VariableType.Ascii, ByteOrder.HostByte, "");
	}

	private void addUnicode()
	{
		addVariable(nextOffset, 4, VariableType.Unicode, ByteOrder.HostByte, "");
	}

	private void addCRC()
	{
		addVariable(nextOffset, 4, VariableType.Int, ByteOrder.HostByte, "");
	}

	private void addVariable(int offset, int length, VariableType variableType, ByteOrder byteOrder, string description = "")
	{
		if (description.Equals("N/A"))
		{
			description = "";
		}
		VariableEditor variableEditor = new(offset, length, variableType, byteOrder, description);
		
		bool? dialogResult = variableEditor.ShowDialog();
		if (dialogResult != true || variableEditor.PacketVariable == null || ActivePacket == null) return;
		
		try
		{
			if (listEnabled)
			{
				variableEditor.PacketVariable.listindex = listIndex;
				listIndex++;
				variableEditor.PacketVariable.listId = listId;
			}

			ActivePacket.myStruct.Add(variableEditor.PacketVariable);
			UpdateWindow(ActivePacket);
		}
		catch {}
	}

	void ListButtonClicked()
	{
		if (listEnabled)
		{
			listEnabled = false;
			ListButtonText = "Start List";
			listId = -1;
			return;
		}
		if (BreakdownSelectedIndex == -1)
		{
			MessageBox.Show("Select an item to be the list counter from the right");
			return;
		}
		listEnabled = true;
		ListButtonText = "Stop List";
		this.listId = ActivePacket.myStruct[BreakdownSelectedIndex].index;
		this.listIndex = 0;
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