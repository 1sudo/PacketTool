using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Point = System.Drawing.Point;

namespace SwgPacketAnalyzer;

public class Settings
{
	public string AppVersion
	{
		get
		{
			return this._AppVersion;
		}
	}

	public Settings()
	{
		this.PcapLogFile = this.AppData + "temp.pcap";
		if (!Directory.Exists(this.AppData))
		{
			Directory.CreateDirectory(this.AppData);
		}
		this.LoadSettings();
	}

	private void LoadSettings()
	{
		Trace.WriteLine(this.AppData + "settings.cfg");
		if (!File.Exists(this.AppData + "settings.cfg"))
		{
			TextWriter textWriter = File.CreateText(this.AppData + "settings.cfg");
			textWriter.WriteLine("-- SWGEmu Packet Analyzer Configuration");
			textWriter.Close();
		}
		List<string> list = this.ReadFileToList(this.AppData + "settings.cfg");
		foreach (string text in list)
		{
			if (text.Contains("="))
			{
				string[] array = text.Replace(" ", "").Split(new char[]
				{
					'='
				});
				this.SetValue(array[0], array[1]);
			}
		}
	}

	private void SetValue(string key, string value)
	{
		if (key == "LastInterface")
		{
			this.LastInterface = value;
		}
		if (key == "LastLoginPort")
		{
			this.LastLoginPort = value;
		}
		if (key == "LastZonePort")
		{
			this.LastZonePort = value;
		}
		if (key == "LastPingPort")
		{
			this.LastPingPort = value;
		}
		if (key == "LastUpdate")
		{
			this.LastUpdate = long.Parse(value);
		}
		if (key == "LastWindowLocation")
		{
			try
			{
				string[] array = value.Split(new char[]
				{
					','
				});
				this.LastWindowLocation = new Point(int.Parse(array[0]), int.Parse(array[1]));
			}
			catch
			{
			}
		}
		this.SaveSettings();
	}

	public void SaveSettings()
	{
	}

	public List<string> ReadFileToList(string fileName)
	{
		List<string> list = new List<string>();
		try
		{
			TextReader textReader = new StreamReader(fileName);
			string text = textReader.ReadLine();
			int num = 0;
			while (text != null)
			{
				if (text.Length > 1 && !text.StartsWith("//") && !text.StartsWith("--"))
				{
					list.Add(text);
					num++;
				}
				text = textReader.ReadLine();
			}
			textReader.Close();
		}
		catch (Exception ex)
		{
            MessageBox.Show("Error!", "Error Reading SettingsFile: " + ex.Message);
		}
		return list;
	}

	public string AppData = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SWGEmuPacketTool");

	private string _AppVersion = "1.0.0";
	public uint defaultLoginPort = 44453U;
	public uint defaultZonePort = 44463U;
	public uint defaultPingPort = 44462U;
	public uint loginPort;
	public uint zonePort;
	public uint pingPort;
	public string PcapLogFile;
	private string LastInterface = "";
	private string LastLoginPort = "";
	private string LastZonePort = "";
	private string LastPingPort = "";
	public long LastUpdate;
	private Point LastWindowLocation;
	public uint SWGKey;
	public string ServerAddress;
}
