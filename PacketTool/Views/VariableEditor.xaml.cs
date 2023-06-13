using System.Windows;
using SwgPacketAnalyzer;

namespace PacketTool.Views;

public partial class VariableEditor : Window
{
    public Variable MyVariable;
    public VariableEditor(int offset, int length, VariableType variableType, ByteOrder byteOrder, string description = "")
    {
        InitializeComponent();
    }
}