using System;
using System.Configuration;


namespace SwgPacketAnalyzer;

public class Util
{
    static string path = AppDomain.CurrentDomain.BaseDirectory.ToString() + "settings.cfg";

    static ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap
    {
        ExeConfigFilename = path
    };
    
    readonly Configuration conf = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

    public string getIP()
    {
            return conf.AppSettings.Settings["IP"].Value;
    }
    
    public string getUserName()
    {
        return conf.AppSettings.Settings["MysqlUserName"].Value;
    }
    
    public string getPassword()
    {
        return conf.AppSettings.Settings["MysqlPassword"].Value;
    }
}
