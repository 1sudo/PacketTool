using System;
using System.Windows;
using Dark.Net;
using PacketTool.ViewModels;
using SwgPacketAnalyzer;

namespace PacketTool.Views;

public enum RichTextBoxType
{
    LineNumber = 0,
    HexEditor = 1,
    AsciiEditor = 2,
    Breakdown = 3
}

public partial class PacketEditor : Window
{
    public PacketEditor(SwgPacket packet)
    {
        InitializeComponent();
        DarkNet.Instance.SetWindowThemeWpf(this, Theme.Dark);
        DataContext = new PacketEditorViewModel(packet);
    }

    private void PacketEditor_OnInitialized(object? sender, EventArgs e)
    {
        PacketEditorViewModel.LineNumberRichTextBox = LineNumbersRichTextBox;
        PacketEditorViewModel.AsciiEditorRichTextBox = AsciiEditorRichTextBox;
        PacketEditorViewModel.HexEditorRichTextBox = HexEditorRichTextBox;
        PacketEditorViewModel.BreakdownRichTextBox = BreakdownRichTextBox;
        
        
        /*PacketEditorViewModel.OnRichTextBox?.Invoke(LineNumbersRichTextBox, RichTextBoxType.LineNumber);
        PacketEditorViewModel.OnRichTextBox?.Invoke(HexEditorRichTextBox, RichTextBoxType.HexEditor);
        PacketEditorViewModel.OnRichTextBox?.Invoke(AsciiEditorRichTextBox, RichTextBoxType.AsciiEditor);
        PacketEditorViewModel.OnRichTextBox?.Invoke(BreakdownRichTextBox, RichTextBoxType.Breakdown);*/
    }
}