using System.Windows;
using Dark.Net;
using PacketTool.ViewModels;
using SwgPacketAnalyzer;

namespace PacketTool.Views;

public partial class VariableEditor : Window
{
    public Variable? PacketVariable { get; set; } = new();

    public VariableEditor(int offset, int length, VariableType variableType, ByteOrder byteOrder, string description = "")
    {
        InitializeComponent();
        DarkNet.Instance.SetWindowThemeWpf(this, Theme.Dark);
        DataContext = new VariableEditorViewModel(offset, length, variableType, byteOrder, this, description);
    }

    public VariableEditor(Variable variable)
    {
        InitializeComponent();
        DarkNet.Instance.SetWindowThemeWpf(this, Theme.Dark);
        DataContext = new VariableEditorViewModel(variable, this);
    }
}