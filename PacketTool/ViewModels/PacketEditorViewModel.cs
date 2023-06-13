using System.Collections.ObjectModel;
using System.Text;
using SwgPacketAnalyzer;

namespace PacketTool.ViewModels;

public class PacketEditorViewModel : ViewModelBase
{
    private SwgPacket SelectedPacket { get; set; }
    
    public PacketEditorViewModel(SwgPacket packet)
    {
        SelectedPacket = packet;
        UpdateWindow(packet);
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
        //this.nextOffset = this.ApplyHighlightsAndBuildStructure(p);
        //this.PreviewVariables(this.nextOffset);
        // HexEditorTextBox.Select(0, 0);
        // this.asciiEditorRichTextBox.Select(0, 0);
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
    
    private string _lineNumbersTextBox;
    private string _hexEditorTextbox;
    private string _asciiEditorTextBox;
    private string _breakdownTextBox;
    private ObservableCollection<string> _breakdownListBox = new();

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
}