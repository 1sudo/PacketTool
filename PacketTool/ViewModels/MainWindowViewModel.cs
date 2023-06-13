using System;
using PacketTool.Models;
using SwgPacketAnalyzer;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using PacketTool.Views;
using SwgPacketAnalyzer.nodes;

namespace PacketTool.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowModel Model { get; }
    
    public IRelayCommand? StartCaptureButton { get; }
    public IRelayCommand? ResumeButton { get; }
    public IRelayCommand? ToolStripMenuOpenButton { get; }
    public IRelayCommand? ToolStripMenuSaveButton { get; }
    public IRelayCommand? ToolStripMenuExitButton { get; }
    public IRelayCommand? ClearButton { get; }
    public IRelayCommand? EditPacketButton { get; }

    public MainWindowViewModel()
    {
        StartCaptureButton = new RelayCommand(StartCapture);
        ResumeButton = new RelayCommand(Resume);
        ToolStripMenuOpenButton = new RelayCommand(OpenDump);
        ToolStripMenuSaveButton = new RelayCommand(SaveDump);
        ToolStripMenuExitButton = new RelayCommand(Exit);
        ClearButton = new RelayCommand(Clear);
        EditPacketButton = new RelayCommand(EditPacket);
        BreakdownTreeViewItems = new ObservableCollection<TreeViewItem>();
        MasterTreeViewItems = new ObservableCollection<TreeViewItem>();
        ErrorTreeViewItems = new ObservableCollection<TreeViewItem>();
        Model = new MainWindowModel(this);
        Model.packetHandler = new PacketHandler(Model);
        Model.EnumerateInterfaces();
    }

    void StartCapture()
    {
        if (Model.packetHandler.livePcapDevice == null)
        {
            MessageBox.Show("Error", "No capture device selected");
            return;
        }
        if (!uint.TryParse(LoginPortText, out Model.settings.loginPort))
        {
            MessageBox.Show("Error", "Invalid Login Port");
            return;
        }
        if (!uint.TryParse(ZonePortText, out Model.settings.zonePort))
        {
            MessageBox.Show("Error", "Invalid Zone Port");
            return;
        }
        if (!uint.TryParse(PingPortText, out Model.settings.pingPort))
        {
            MessageBox.Show("Error", "Invalid Ping Port");
            return;
        }
        
        if (StartCaptureButtonText == "Start Capture")
        {
            if (!Model.packetHandler.StartCapture()) return;
            
            ResumeButtonEnabled = true;
            OpenToolStripMenuItemEnabled = false;
            InterfaceBoxEnabled = false;
            ClearButtonEnabled = true;
            StartCaptureButtonText = "Stop Capture";
            
            if (Model.packetHandler.LoggingEnabled)
            {
                Title = "SWGEmu Packet Tool - Capturing";
                return;
            }
            
            Title = "SWGEmu Packet Tool - Paused";
        }
        else if (Model.packetHandler.StopCapture())
        {
            ResumeButtonText = "Resume";
            StartCaptureButtonText = "Start Capture";
            Title = "SWGEmu Packet Tool - Paused";
            ResumeButtonEnabled = false; 
            OpenToolStripMenuItemEnabled = true;
            InterfaceBoxEnabled = true;
            ClearButtonEnabled = false;
        }
    }

    void Resume()
    {
        if (ResumeButtonText == "Resume")
        {
            Model.packetHandler.LoggingEnabled = true;
            ResumeButtonText = "Pause";
            Title = "SWGEmu Packet Tool - Capturing";
            return;
        }
        Model.packetHandler.LoggingEnabled = false;
        ResumeButtonText = "Resume";
        Title = "SWGEmu Packet Tool - Paused";
    }

    void OpenDump()
    {
        Model.OpenDump();
    }

    void SaveDump()
    {
        Model.packetHandler.SaveDump();
    }

    public void Clear()
    {
        BreakdownTreeViewItems.Clear();
        ErrorTreeViewItems.Clear();
        MasterTreeViewItems.Clear();
        PacketViewText = "";
        BreakDownViewText = "";
        Model.packetHandler.Clear();
    }

    private void EditPacket()
    {
        SwgPacket gamePacket = ((SwgPacketTreeNode)Model.selectedNode).getGamePacket();
        PacketEditor packetEditor = new(gamePacket);
        packetEditor.ShowDialog();
    }

    void Exit()
    {
        Environment.Exit(0);
    }
    
    private ObservableCollection<string> _availableInterfaces;
    private ObservableCollection<TreeViewItem> _breakdownTreeViewItems;
    private ObservableCollection<TreeViewItem> _masterTreeViewItems;
    private ObservableCollection<TreeViewItem> _errorTreeViewItems;
    private int _availableInterfacesIndex;
    private string _startCaptureButtonText = "Start Capture";
    private string _resumeButtonText = "Resume";
    private string _sessionKey;
    private string _gameServerAddress;
    private string _loginPortText = "44453";
    private string _zonePortText = "44464";
    private string _pingPortText = "44462";
    private string _title = "SWGEmu Packet Tool";
    private bool _resumeButtonEnabled = true;
    private bool _openToolStripMenuItemEnabled = true;
    private bool _interfaceBoxEnabled = true;
    private bool _clearButtonEnabled = true;
    private string _packetViewText;
    private string _breakDownViewText;

    public ObservableCollection<string> AvailableInterfaces
    {
        get => _availableInterfaces;
        set => SetProperty(ref _availableInterfaces, value);
    }

    public ObservableCollection<TreeViewItem> BreakdownTreeViewItems
    {
        get => _breakdownTreeViewItems;
        set => SetProperty(ref _breakdownTreeViewItems, value);
    }
    
    public ObservableCollection<TreeViewItem> MasterTreeViewItems
    {
        get => _masterTreeViewItems;
        set => SetProperty(ref _masterTreeViewItems, value);
    }
    
    public ObservableCollection<TreeViewItem> ErrorTreeViewItems
    {
        get => _errorTreeViewItems;
        set => SetProperty(ref _errorTreeViewItems, value);
    }

    public int AvailableInterfacesIndex
    {
        get => _availableInterfacesIndex;
        set
        {
            SetProperty(ref _availableInterfacesIndex, value);
            Model.UpdatePcapDevice(value);
        }
    }

    public string StartCaptureButtonText
    {
        get => _startCaptureButtonText;
        set => SetProperty(ref _startCaptureButtonText, value);
    }

    public string ResumeButtonText
    {
        get => _resumeButtonText;
        set => SetProperty(ref _resumeButtonText, value);
    }

    public string SessionKey
    {
        get => _sessionKey;
        set => SetProperty(ref _sessionKey, value);
    }

    public string GameServerAddress
    {
        get => _gameServerAddress;
        set => SetProperty(ref _gameServerAddress, value);
    }

    public string LoginPortText
    {
        get => _loginPortText;
        set => SetProperty(ref _loginPortText, value);
    }

    public string ZonePortText
    {
        get => _zonePortText;
        set => SetProperty(ref _zonePortText, value);
    }

    public string PingPortText
    {
        get => _pingPortText;
        set => SetProperty(ref _pingPortText, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public bool ResumeButtonEnabled
    {
        get => _resumeButtonEnabled;
        set => SetProperty(ref _resumeButtonEnabled, value);
    }

    public bool OpenToolStripMenuItemEnabled
    {
        get => _openToolStripMenuItemEnabled;
        set => SetProperty(ref _openToolStripMenuItemEnabled, value);
    }

    public bool InterfaceBoxEnabled
    {
        get => _interfaceBoxEnabled;
        set => SetProperty(ref _interfaceBoxEnabled, value);
    }

    public bool ClearButtonEnabled
    {
        get => _clearButtonEnabled;
        set => SetProperty(ref _clearButtonEnabled, value);
    }

    public string PacketViewText
    {
        get => _packetViewText;
        set => SetProperty(ref _packetViewText, value);
    }

    public string BreakDownViewText
    {
        get => _breakDownViewText;
        set => SetProperty(ref _breakDownViewText, value);
    }
};
