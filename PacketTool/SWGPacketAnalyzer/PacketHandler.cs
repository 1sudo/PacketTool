using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using PacketDotNet;
using PacketTool.Models;
using SharpPcap;
using SharpPcap.LibPcap;
using SwgPacketAnalyzer.nodes;
using SwgPacketAnalyzer.packetqueues;
using SwgPacketAnalyzer.packets;
using SwgPacketAnalyzer.SwgPackets;

namespace SwgPacketAnalyzer
{
	public class PacketHandler
	{
		// CaptureFileWriterDevice writer = new(settings.PcapLogFile);
		
		public PacketHandler(MainWindowModel mainWindowModel)
		{
			model = mainWindowModel;
			settings = mainWindowModel.settings;
			instance = this;
		}

		public void reset()
		{
			try
			{
				this.packetNumber = 0;
				PacketHandler.settings.SWGKey = 0U;
			}
			catch (Exception ex)
			{
                MessageBox.Show("Error!", "Error resetting packet handler: " + ex.Message);
			}
		}

		public void Clear()
		{
			this.CompleteList.Clear();
			this.packetQueues.ClearAll();
			this.SOEPacketNodes.Clear();
			this.SWGPacketNodes.Clear();
			this.ErrorPacketNodes.Clear();
			if (this.livePcapDevice != null && this.livePcapDevice.Opened)
			{
				livePcapDevice.Close();
				livePcapDevice.Open();
			}
		}

		public bool StartCapture()
		{
			Trace.WriteLine("Started Capture!");
			bool result;
			try
			{
				reset();
				Clear();
				onlineCapture = true;
				sessionEstablished = false;
				livePcapDevice.OnPacketArrival += device_OnPacketArrival;
				int read_timeout = 1000;
				livePcapDevice.Open(DeviceModes.Promiscuous, read_timeout);
				string filter = string.Concat(new object[]
				{
					"ip and udp and ((src or dst port ",
					PacketHandler.settings.pingPort,
					") or (src or dst port ",
					PacketHandler.settings.defaultPingPort,
					") or (src or dst port ",
					PacketHandler.settings.zonePort,
					") or (src or dst port ",
					PacketHandler.settings.defaultZonePort,
					") or (src or dst port ",
					PacketHandler.settings.loginPort,
					") or (src or dst port ",
					PacketHandler.settings.defaultLoginPort,
					"))"
				});
				this.livePcapDevice.Filter = filter;
				this.livePcapDevice.StartCapture();
				File.Delete(PacketHandler.settings.PcapLogFile);
				this.livePcapDevice.Open();
				result = true;
			}
			catch (Exception ex)
			{
                MessageBox.Show("Error!", "Error starting capture: " + ex.Message);
				result = false;
			}
			
			return result;
		}

		public bool StopCapture()
		{
			bool result;
			try
			{
				if (this.livePcapDevice != null)
				{
					this.livePcapDevice.StopCapture();
					this.livePcapDevice.Close();
				}
				this.LoggingEnabled = false;
				result = true;
			}
			catch (Exception ex)
			{
                MessageBox.Show("Error!", "Error stopping capture: " + ex.Message);
				result = false;
			}
			return result;
		}

		private void device_OnPacketArrival(object sender, PacketCapture packet)
		{
			/*if (LoggingEnabled)
			{
				writer.Write(packet.Data);
			}*/

           /* Trace.WriteLine("Got Packet!");
            Trace.WriteLine(Encoding.UTF8.GetString(packet.Data.ToArray()));
			*/

            processPacket((PcapDevice)sender, Packet.ParsePacket(LinkLayers.Ethernet, packet.Data.ToArray()));
		}

		private void processPacket(PcapDevice device, Packet packet)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
                try
                {
                    SoePacket soepacket = this.CreateNewSOEPacket(packet, false, true);
                    if (soepacket is ErrorPacket)
                    {
                        if (this.LoggingEnabled)
                        {
                            model.UpdateErrorView(new ErrorPacketTreeNode((ErrorPacket)soepacket), this.onlineCapture);
                        }
                    }
                    else
                    {
                        if (soepacket is SessionRequest)
                        {
                            this.handleSessionRequest(device, soepacket);
                        }
                        if (soepacket is SessionResponse)
                        {
                            this.handleSessionResponse(soepacket);
                        }
                        if (soepacket is SessionRequest || soepacket is SessionResponse || this.sessionEstablished || PacketHandler.settings.SWGKey == 0U)
                        {
                            if (this.LoggingEnabled)
                            {
                                this.addToCompleteTreenode(soepacket);
                                foreach (SoePacket spacket in soepacket.SOEPacketList)
                                {
                                    this.packetQueues.addPacket(spacket);
                                }
                                if (soepacket.SOEPacketList.Count == 0)
                                {
                                    this.packetQueues.addPacket(soepacket);
                                }
                                lock (this.packetQueues)
                                {
                                    foreach (PacketQueue packetQueue in this.packetQueues.getQueues())
                                    {
                                        while (packetQueue.Count > 0)
                                        {
                                            this.HandleGamePacket(device, packetQueue);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Concat(new object[]
                    {
                    "Error in processPacket, packet ",
                    this.packetNumber,
                    ": ",
                    ex.Message,
                    "\n",
                    ex.StackTrace
                    }), "Error!");
                }
            });

		}

		private void processOfflinePacket(PcapDevice device, Packet packet)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
                try
                {
                    SoePacket soepacket = this.CreateNewSOEPacket(packet, false, true);
                    if (soepacket is ErrorPacket && this.LoggingEnabled)
                    {
                        this.ErrorPacketNodes.Add(new ErrorPacketTreeNode((ErrorPacket)soepacket));
                    }
                    if (soepacket is SessionRequest)
                    {
                        this.handleSessionRequest(device, soepacket);
                    }
                    if (soepacket is SessionResponse)
                    {
                        this.handleSessionResponse(soepacket);
                    }
                    if (soepacket is SessionRequest || soepacket is SessionResponse || this.sessionEstablished || PacketHandler.settings.SWGKey == 0U)
                    {
                        if (this.LoggingEnabled)
                        {
                            this.addToCompleteTreenodeOffline(soepacket);
                            foreach (SoePacket spacket in soepacket.SOEPacketList)
                            {
                                this.packetQueues.addPacket(spacket);
                            }
                            if (soepacket.SOEPacketList.Count == 0)
                            {
                                this.packetQueues.addPacket(soepacket);
                            }
                            lock (this.packetQueues)
                            {
                                foreach (PacketQueue packetQueue in this.packetQueues.getQueues())
                                {
                                    while (packetQueue.Count > 0)
                                    {
                                        this.HandleGamePacketOffline(device, packetQueue);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error!", string.Concat(new object[]
                    {
                    "Error in processPacket, packet ",
                    this.packetNumber,
                    ": ",
                    ex.Message,
                    "\n",
                    ex.StackTrace
                    }));
                }
            });
		}

		private void handleSessionRequest(PcapDevice device, SoePacket soePacket)
		{
			PacketHandler.settings.ServerAddress = soePacket.getDestinationAddress();

			if (PacketHandler.settings.ServerAddress.StartsWith("199.108"))
			{
				PacketHandler.isLiveServer = true;
			}
			else
			{
				PacketHandler.isLiveServer = false;
			}
			if (device is LibPcapLiveDevice)
			{
				string filter = string.Concat(new object[]
				{
					"ip and udp and ((src or dst port ",
					PacketHandler.settings.pingPort,
					") or (src or dst port ",
					PacketHandler.settings.defaultPingPort,
					") or (src or dst port ",
					PacketHandler.settings.zonePort,
					") or (src or dst port ",
					PacketHandler.settings.defaultZonePort,
					") or (src or dst port ",
					PacketHandler.settings.loginPort,
					") or (src or dst port ",
					PacketHandler.settings.defaultLoginPort,
					"))"
				});
				device.Filter = filter;
			}
			model.UpdateServerAddress(PacketHandler.settings.ServerAddress);
		}

		private void handleSessionResponse(SoePacket soePacket)
		{
			this.sessionEstablished = true;
			SessionResponse sessionResponse = (SessionResponse)soePacket;
			PacketHandler.settings.SWGKey = sessionResponse.SessionKey;
			model.UpdateSessionKey(PacketHandler.settings.SWGKey);
		}

		public SoePacket CreateNewSOEPacket(Packet packet, bool preproccessed, bool increasePacketNumber = true)
		{
			UdpPacket encapsulated = packet.Extract<UdpPacket>();
			if (increasePacketNumber)
			{
				this.packetNumber++;
			}
			ushort opcode = this.GetOpcode(encapsulated);
			ushort num = opcode;
			switch (num)
			{
			case 1:
				return new SessionRequest(this.packetNumber, packet, preproccessed);
			case 2:
				return new SessionResponse(this.packetNumber, packet, preproccessed);
			case 3:
				return new MultiPacket(this.packetNumber, packet, preproccessed);
			case 4:
			case 10:
			case 11:
			case 12:
			case 14:
			case 15:
			case 16:
			case 18:
			case 19:
			case 20:
				break;
			case 5:
				return new Disconnect(this.packetNumber, packet, preproccessed);
			case 6:
				return new KeepAlive(this.packetNumber, packet, preproccessed);
			case 7:
				return new NetStatusRequest(this.packetNumber, packet, preproccessed);
			case 8:
				return new NetStatusResponse(this.packetNumber, packet, preproccessed);
			case 9:
				return new DataPacket(this.packetNumber, packet, preproccessed);
			case 13:
				return new FragmentedPacket(this.packetNumber, packet, preproccessed);
			case 17:
				return new OutOfOrderPacket(this.packetNumber, packet, preproccessed);
			case 21:
				return new Acknowledge(this.packetNumber, packet, preproccessed);
			default:
				switch (num)
				{
				case 29:
					return new SeriousErrorReply(this.packetNumber, packet, preproccessed);
				case 30:
					return new SeriousErrorAcknowledge(this.packetNumber, packet, preproccessed);
				}
				break;
			}
			SoePacket soepacket = new UnknownPacket(this.packetNumber, packet, preproccessed);
			if (soepacket.getServerOrigin() == "Ping")
			{
				soepacket = new KeepAlive(this.packetNumber, packet, preproccessed);
			}
			else
			{
				soepacket = new StandalonePacket(this.packetNumber, packet, preproccessed);
				if (!((StandalonePacket)soepacket).IsValid())
				{
					soepacket = new UnknownPacket(this.packetNumber, packet, preproccessed);
				}
			}
			return soepacket;
		}

		private ushort GetOpcode(UdpPacket upacket)
		{
			ushort num = 0;
			int num2 = 1;
			int num3 = 0;
			do
			{
				byte b = upacket.PayloadData[num2];
				num += (ushort)(b << num3 * 8);
				num3++;
				num2--;
			}
			while (num2 >= 0);
			return num;
		}

		public SwgPacket createNewGamePacket(List<byte> Data, int offset, int length, int packetNum)
		{
			if (offset + 5 <= Data.Count)
			{
				if (Data[offset + 2] == 12 && Data[offset + 3] == 95 && Data[offset + 4] == 167 && Data[offset + 5] == 104)
				{
					return new BaselinesPacket(Data, offset, length, packetNum);
				}
				if (Data[offset + 2] == 83 && Data[offset + 3] == 33 && Data[offset + 4] == 134 && Data[offset + 5] == 18)
				{
					return new DeltasPacket(Data, offset, length, packetNum);
				}
				if (Data[offset + 2] == 70 && Data[offset + 3] == 94 && Data[offset + 4] == 206 && Data[offset + 5] == 128)
				{
					return new ObjectControllerPacket(Data, offset, length, packetNum);
				}
			}
			return new SwgPacket(Data, offset, length, packetNum);
		}

		private void HandleGamePacket(PcapDevice device, PacketQueue queue)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
                SwgPacket swgpacket = queue[0];
                if (this.LoggingEnabled && !this.isFiltered(swgpacket))
                {
                    SwgPacketTreeNode newNode = new SwgPacketTreeNode(swgpacket, false);
                    model.UpdateBreakdownList(newNode, this.onlineCapture);
                }
                queue.Remove(swgpacket);
            });
		}

		private void HandleGamePacketOffline(PcapDevice device, PacketQueue queue)
		{
            Application.Current.Dispatcher.Invoke(() =>
            {
                SwgPacket swgpacket = queue[0];
                if (this.LoggingEnabled && !this.isFiltered(swgpacket))
                {
                    SwgPacketTreeNode item = new SwgPacketTreeNode(swgpacket, false);
                    this.SWGPacketNodes.Add(item);
                }
                queue.Remove(swgpacket);
            });
		}

		private bool isFiltered(SwgPacket gamePacket)
		{
			// TODO - filter
			//return this.mainForm.isFilterEnabled() && !gamePacket.Name.ToLower().Contains(this.mainForm.GetFilter().ToLower());
			return false;
		}

		private void addToCompleteTreenode(SoePacket soePacket)
		{
			Application.Current.Dispatcher.Invoke(() => {
                this.CompleteList.Add(soePacket);
                bool flag = false;
                while (this.CompleteList.Count > PacketHandler.maxListSize)
                {
                    this.packetQueues.remove(this.CompleteList[0]);
                    this.CompleteList.RemoveAt(0);
                    flag = true;
                }
                if (flag)
                {
                    GC.Collect();
                }
                SoePacketTreeNode newNode = new(soePacket);
				newNode.VerticalContentAlignment = VerticalAlignment.Center;
				newNode.HorizontalContentAlignment = HorizontalAlignment.Center;
                model.UpdateMasterPacketList(newNode, this.onlineCapture);
            });
        }

		private void addToCompleteTreenodeOffline(SoePacket soePacket)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
                this.CompleteList.Add(soePacket);
                SoePacketTreeNode item = new SoePacketTreeNode(soePacket);
                this.SOEPacketNodes.Add(item);
            });
		}

		public void SaveDump()
		{
            if (this.livePcapDevice != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.DefaultExt = ".pcap";
                saveFileDialog.Filter = "PCAP Files|*.pcap";
                saveFileDialog.CheckPathExists = true;
                saveFileDialog.OverwritePrompt = true;
                if (saveFileDialog.ShowDialog() != true)
                {
                    return;
                }
                
                try
                {
                    File.Delete(saveFileDialog.FileName);
                    File.Copy(PacketHandler.settings.PcapLogFile, saveFileDialog.FileName);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message ?? "");
                    return;
                }
            }
            MessageBox.Show("No dump exists, cannot save");
        }

		public void CaptureOfflinePackets(string filename)
		{
			try
			{
				Trace.WriteLine("Starting offline capture");
				this.LoggingEnabled = true;
				CaptureFileReaderDevice offlinePcapDevice = new(filename);
				offlinePcapDevice.Open();
				Trace.WriteLine("Initialized Offline Capture device");
				this.reset();
				this.Clear();
				this.onlineCapture = false;
				Trace.WriteLine("Cleared objets");
				string filter = string.Concat(new object[]
				{
					"ip and udp and ((src or dst port ",
					PacketHandler.settings.pingPort,
					") or (src or dst port ",
					PacketHandler.settings.defaultPingPort,
					") or (src or dst port ",
					PacketHandler.settings.zonePort,
					") or (src or dst port ",
					PacketHandler.settings.defaultZonePort,
					") or (src or dst port ",
					PacketHandler.settings.loginPort,
					") or (src or dst port ",
					PacketHandler.settings.defaultLoginPort,
					"))"
				});
				offlinePcapDevice.Filter = filter;
				Trace.WriteLine("Initialized Filter");
				File.Delete(PacketHandler.settings.PcapLogFile);
				Trace.WriteLine("Initialized Log File");
				double totalMilliseconds = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
				Trace.WriteLine("Starting capture");
				int num = 1;
				
				while (true)
				{
					offlinePcapDevice.GetNextPacket(out PacketCapture nextPacket);
					RawCapture? packet = nextPacket.GetPacket();

					if (packet is null) break;
					
					this.processOfflinePacket(offlinePcapDevice, Packet.ParsePacket(LinkLayers.Ethernet, packet.Data));
					num++;
				}
				
				double totalMilliseconds2 = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
				
				Trace.WriteLine(string.Concat(new object[]
				{
					"Capture Complete: ",
					num,
					" packets took ",
					totalMilliseconds2 - totalMilliseconds,
					" milliseconds"
				}));
				
				offlinePcapDevice.Close();
				Trace.WriteLine("Cleanup Completed");
				Trace.WriteLine("Starting GUI Update");
				totalMilliseconds = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
				model.UpdateGUIForOffline(this.SOEPacketNodes, this.SWGPacketNodes, this.ErrorPacketNodes);
				Trace.WriteLine("Starting GUI Update");
				totalMilliseconds2 = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds;
				Trace.WriteLine(string.Concat(new object[]
				{
					"GUI Render complete : ",
					num,
					" packets took ",
					totalMilliseconds2 - totalMilliseconds,
					" milliseconds"
				}));
				this.LoggingEnabled = false;
			}
			catch (Exception ex)
			{
                MessageBox.Show("Error!", "Error in offline capture: " + ex.Message);
				this.LoggingEnabled = false;
			}
		}

		public static PacketHandler instance;
		private MainWindowModel model;
		public LibPcapLiveDevice livePcapDevice;
		public bool LoggingEnabled;
		private bool sessionEstablished;
		public static Settings settings = null;
		public static bool isLiveServer = false;
		public string serverAddress = "";
		private int packetNumber;
		public static int maxListSize = 5000;
		private List<SoePacket> CompleteList = new List<SoePacket>();
		private PacketQueues packetQueues = new PacketQueues();
		private bool onlineCapture;
		private List<SoePacketTreeNode> SOEPacketNodes = new List<SoePacketTreeNode>();
		private List<SwgPacketTreeNode> SWGPacketNodes = new List<SwgPacketTreeNode>();
		private List<ErrorPacketTreeNode> ErrorPacketNodes = new List<ErrorPacketTreeNode>();
	}
}
