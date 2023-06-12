using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using PacketTool.ViewModels;
using SharpPcap.LibPcap;
using SwgPacketAnalyzer;
using SwgPacketAnalyzer.nodes;
using SwgPacketAnalyzer.variables;

namespace PacketTool.Models;

public class MainWindowModel
{
    public MainWindowViewModel ViewModel { get; }
    public static Action<TreeViewItem> OnNodeClicked;

    public MainWindowModel(MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        OnNodeClicked += NodeClicked;
    }
    
    internal Settings? settings = new();
    internal PacketHandler packetHandler;
    internal IPacketNode selectedNode;
    public static SortedDictionary<string, VariableDictionary> PacketVariables = new();
    private static SortedDictionary<uint, string> CRCTable = new();
    
    public static string CrcLookup(uint crc) => CRCTable.ContainsKey(crc) ? CRCTable[crc] : "";

    public void UpdateMasterPacketList(List<IPacketNode> nodes, bool online)
    {
        foreach (IPacketNode packetNode in nodes)
        {
            UpdateMasterPacketList(packetNode, online);
        }
    }

    public void UpdateMasterPacketList(IPacketNode newNode, bool online)
    {
        if (!(newNode is SoePacketTreeNode))
        {
            MessageBox.Show("Error!", "Trying to add something that isn't an SOE packet to the treeview");
            return;
        }
        
        ViewModel.MasterTreeViewItems.Add((TreeViewItem)newNode);
        
        
        while (ViewModel.MasterTreeViewItems.Count > PacketHandler.maxListSize)
        {
            ViewModel.MasterTreeViewItems.RemoveAt(0);
        }

        // Don't know what this is? Scroll packets maybe?
        /*if (this.followCheckBox.Checked)
        {
            ((TreeNode)newNode).EnsureVisible();
        }*/
    }

    private void UpdateMasterListOffline(List<IPacketNode> nodes, bool online)
    {
        foreach (IPacketNode packetNode in nodes)
        {
            if (!(packetNode is SoePacketTreeNode))
            {
                MessageBox.Show("Error!", "Trying to add something that isn't an SOE packet to the treeview");
                break;
            }
            
            ViewModel.MasterTreeViewItems.Add((TreeViewItem)packetNode);
        }
    }

    public void UpdateBreakdownList(List<IPacketNode> nodes, bool online)
    {
        foreach (IPacketNode packetNode in nodes)
        {
            UpdateBreakdownList(packetNode, online);
        }
    }
    
    private void UpdateBreakdownPacketListOffline(List<IPacketNode> nodes, bool online)
    {
        foreach (IPacketNode newNode in nodes)
        {
            UpdateBreakdownList(newNode, true);
        }
    }

    public void UpdateBreakdownList(IPacketNode newNode, bool online)
    {
        if (!(newNode is SwgPacketTreeNode))
        {
            MessageBox.Show("Error!", "Trying to add something that isn't an SWG packet to the treeview");
            return;
        }

        var header = ((TreeViewItem)newNode).Header.ToString();
        
        // TODO - filter button
        /*if (this.filterButton.Text == "Filter ON" && !header.ToLower().Contains(this.GetFilter().ToLower()))
        {
            return;
        }*/
        
        ViewModel.BreakdownTreeViewItems.Add((TreeViewItem)newNode);

        int packetNumber = ((SoePacketTreeNode)ViewModel.MasterTreeViewItems[0]).getPacket().getPacketNumber();
        while (((SwgPacketTreeNode)ViewModel.BreakdownTreeViewItems[0]).getGamePacket().PacketNum < packetNumber)
        {
            ViewModel.BreakdownTreeViewItems.RemoveAt(0);
        }
        
        // Don't know what this is? Scroll packets maybe?
        /*if (this.followCheckBox.Checked)
        {
            this.breakdownTreeview.Nodes[this.breakdownTreeview.Nodes.Count - 1].EnsureVisible();
        }*/
    }

    public void UpdateErrorView(IPacketNode newNode, bool online)
    {
        UpdateErrorPacketList(newNode, online);
    }

    private void UpdateErrorPacketList(IPacketNode newNode, bool online)
    {
        if (!(newNode is ErrorPacketTreeNode))
        {
            _ = MessageBox.Show("Error!", "Trying to add something that isn't an Error packet to the treeview");
            return;
        }
        
        ViewModel.ErrorTreeViewItems.Add((TreeViewItem)newNode);
        
        // Don't know what this is? Scroll packets maybe?
        /*if (this.followCheckBox.Checked)
        {
            ((TreeViewItem)newNode).EnsureVisible();
        }*/
    }

    internal void UpdateGUIForOffline(List<SoePacketTreeNode> soePacketNodes, List<SwgPacketTreeNode> swgPacketNodes, List<ErrorPacketTreeNode> errorPacketNodes)
    {
        MakeOfflineGUI(soePacketNodes, swgPacketNodes, errorPacketNodes);
    }

    internal void MakeOfflineGUI(List<SoePacketTreeNode> soePacketNodes, List<SwgPacketTreeNode> swgPacketNodes, List<ErrorPacketTreeNode> errorPacketNodes)
    {
        ViewModel.BreakdownTreeViewItems = new ObservableCollection<TreeViewItem>(swgPacketNodes);
        ViewModel.MasterTreeViewItems = new ObservableCollection<TreeViewItem>(soePacketNodes);
        ViewModel.ErrorTreeViewItems = new ObservableCollection<TreeViewItem>(errorPacketNodes);
    }

    public void UpdateSessionKey(uint key)
    {
        ViewModel.SessionKey = key.ToString("X8");
        settings.SWGKey = key;
    }

    public void UpdateServerAddress(string address)
    {
        ViewModel.GameServerAddress = address;
    }

    internal void EnumerateInterfaces()
    {
        ViewModel.AvailableInterfaces = new();
        ViewModel.StartCaptureButtonText = "Start Capture";
        ViewModel.ResumeButtonText = "Resume";
        
        LibPcapLiveDeviceList instance = LibPcapLiveDeviceList.Instance;
        
        if (instance.Count < 1)
        {
            ViewModel.AvailableInterfaces.Add("No devices were found on this machine");
        }
        else
        {
            foreach (LibPcapLiveDevice livePcapDevice in instance)
            {
                ViewModel.AvailableInterfaces.Add(livePcapDevice.Description + ": " + livePcapDevice.Interface.FriendlyName);
            }
            
            ViewModel.AvailableInterfacesIndex = 0;
        }
    }

    internal void UpdatePcapDevice(int idx)
    {
        LibPcapLiveDeviceList instance = LibPcapLiveDeviceList.Instance;
        packetHandler.livePcapDevice = instance[idx];
    }

    internal void OpenDump()
    {
        try
        {
            try
            {
                settings.loginPort = uint.Parse(ViewModel.LoginPortText);
                settings.zonePort = uint.Parse(ViewModel.ZonePortText);
                settings.pingPort = uint.Parse(ViewModel.PingPortText);
            }
            catch
            {
                MessageBox.Show("Error!", string.Concat(new object[]
                {
                    "Login Port: ",
                    settings.loginPort,
                    "   Zone Port: ",
                    settings.zonePort,
                    "   Ping Port: ",
                    settings.pingPort
                }));
            }
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = ".pcap";
                openFileDialog.Filter = "PCAP Files|*.pcap;*.cap";
                openFileDialog.CheckPathExists = true;
                if (openFileDialog.ShowDialog() == true)
                {
                    if (File.Exists(openFileDialog.FileName))
                    {
                        ViewModel.Clear();
                        if (packetHandler == null)
                        {
                            MessageBox.Show("Packet Handler is missing for some reason");
                        }
                        else
                        {
                            ViewModel.Clear();
                            packetHandler.CaptureOfflinePackets(openFileDialog.FileName);
                        }
                    }
                    else
                    {
                        MessageBox.Show("File not found, please try again");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error!","Unable to open file browser: " + ex.Message);
            }
        }
        catch (Exception ex2)
        {
            MessageBox.Show("Error!",ex2.Message);
        }
    }

    private void NodeClicked(TreeViewItem node)
    {
        ViewModel.PacketViewText = "";
        ViewModel.BreakDownViewText = "";
        IPacketNode packetNode = (IPacketNode)node;
        selectedNode = packetNode;

        if (packetNode == null) return;

        StringBuilder packetViewSb = new();
        foreach (var line in packetNode.GetDisplayView())
        {
            packetViewSb.AppendLine(line);
        }

        StringBuilder breakDownViewSb = new();
        foreach (var line in packetNode.GetPacketBreakdown())
        {
            breakDownViewSb.AppendLine(line);
        }

        ViewModel.PacketViewText = packetViewSb.ToString();
        ViewModel.BreakDownViewText = breakDownViewSb.ToString();
    }
}