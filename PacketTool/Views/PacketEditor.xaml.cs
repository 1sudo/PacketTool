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
}