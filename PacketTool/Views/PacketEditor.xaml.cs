using System;
using System.Windows;
using Dark.Net;
using PacketTool.ViewModels;
using SwgPacketAnalyzer;

namespace PacketTool.Views;

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
    }
}