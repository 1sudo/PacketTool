using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using PacketTool.Views;
using SwgPacketAnalyzer;

namespace PacketTool.ViewModels;

public class VariableEditorViewModel : ViewModelBase
{
    public IRelayCommand? SaveButton { get; }
    private VariableEditor Window { get; set; }
    
    public VariableEditorViewModel(int offset, int length, VariableType variableType, ByteOrder byteOrder, VariableEditor window, string description = "")
    {
        Window = window;
        SaveButton = new RelayCommand(Save);
        Window.PacketVariable.index = offset;
        Window.PacketVariable.setLength(length);
        Window.PacketVariable.Type = variableType;
        Window.PacketVariable.ByteOrder = byteOrder;
        Window.PacketVariable.ShowValue = true;
        Window.PacketVariable.Description = description;
        PopulateForm();
    }

    public VariableEditorViewModel(Variable variable, VariableEditor window)
    {
        Window = window;
        SaveButton = new RelayCommand(Save);
        
        Window.PacketVariable = new Variable
        {
            index = variable.index,
            Type = variable.Type,
            ByteOrder = variable.ByteOrder,
            Name = variable.Name,
            Description = variable.Description,
            Notes = variable.Notes,
            ShowValue = variable.ShowValue,
        };
        Window.PacketVariable.setLength(variable.getLength());
        Window.PacketVariable.setComplete(variable.isComplete());
        PopulateForm();
    }
    
    private void PopulateForm()
    {
        if (Window.PacketVariable is null) return;
        TypeComboBoxCollection = new ObservableCollection<string>(Enum.GetNames(typeof(VariableType)).ToList());
        ByteComboBoxSelection = new ObservableCollection<string>(Enum.GetNames(typeof(ByteOrder)).ToList());
        NameTextBox = Window.PacketVariable.Name;
        DescriptionTextBox = Window.PacketVariable.Description;
        NotesTextBox = Window.PacketVariable.Notes;
        TypeComboBoxSelectedItem = Window.PacketVariable.Type.ToString();
        ByteComboBoxSelectedItem = Window.PacketVariable.ByteOrder.ToString();
        ShowValueCheckBoxChecked = Window.PacketVariable.ShowValue;
        CompleteCheckBoxChecked = Window.PacketVariable.isComplete();
    }

    private void Save()
    {
        if (Window.PacketVariable is null) return;
        Window.PacketVariable.Type = (VariableType)Enum.Parse(typeof(VariableType), TypeComboBoxSelectedItem);
        switch (Window.PacketVariable.getType())
        {
            case VariableType.Byte:
                Window.PacketVariable.setLength(1);
                break;
            case VariableType.Short:
                Window.PacketVariable.setLength(2);
                break;
            case VariableType.Int:
                Window.PacketVariable.setLength(4);
                break;
            case VariableType.Float:
                Window.PacketVariable.setLength(4);
                break;
            case VariableType.Long:
                Window.PacketVariable.setLength(8);
                break;
            case VariableType.Ascii:
                Window.PacketVariable.setLength(2);
                break;
            case VariableType.Unicode:
                Window.PacketVariable.setLength(4);
                break;
        }
        Window.PacketVariable.ByteOrder = (ByteOrder)Enum.Parse(typeof(ByteOrder), ByteComboBoxSelectedItem);
        Window.PacketVariable.Name = NameTextBox;
        Window.PacketVariable.Description = DescriptionTextBox;
        Window.PacketVariable.Notes = NotesTextBox;
        Window.PacketVariable.setComplete(CompleteCheckBoxChecked);
        Window.PacketVariable.ShowValue = ShowValueCheckBoxChecked;
        Window.DialogResult = true;
    }

    private ObservableCollection<string> _typeComboBoxCollection;
    private ObservableCollection<string> _byteComboBoxCollection;
    private string _typeComboBoxSelectedItem;
    private string _byteComboBoxSelectedItem;
    private bool _showValueCheckBoxChecked;
    private bool _completeCheckBoxChecked;
    private string _nameTextbox;
    private string _descriptionTextBox;
    private string _notesTextbox;

    public ObservableCollection<string> TypeComboBoxCollection
    {
        get => _typeComboBoxCollection;
        set => SetProperty(ref _typeComboBoxCollection, value);
    }

    public ObservableCollection<string> ByteComboBoxSelection
    {
        get => _byteComboBoxCollection;
        set => SetProperty(ref _byteComboBoxCollection, value);
    }

    public string TypeComboBoxSelectedItem
    {
        get => _typeComboBoxSelectedItem;
        set => SetProperty(ref _typeComboBoxSelectedItem, value);
    }

    public string ByteComboBoxSelectedItem
    {
        get => _byteComboBoxSelectedItem;
        set => SetProperty(ref _byteComboBoxSelectedItem, value);
    }

    public bool ShowValueCheckBoxChecked
    {
        get => _showValueCheckBoxChecked;
        set => SetProperty(ref _showValueCheckBoxChecked, value);
    }

    public bool CompleteCheckBoxChecked
    {
        get => _completeCheckBoxChecked;
        set => SetProperty(ref _completeCheckBoxChecked, value);
    }

    public string NameTextBox
    {
        get => _nameTextbox;
        set => SetProperty(ref _nameTextbox, value);
    }

    public string DescriptionTextBox
    {
        get => _descriptionTextBox;
        set => SetProperty(ref _descriptionTextBox, value);
    }

    public string NotesTextBox
    {
        get => _notesTextbox;
        set => SetProperty(ref _notesTextbox, value);
    }
}