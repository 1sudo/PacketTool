using System;
using System.Windows;
using Dark.Net;

namespace PacketTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            DarkNet.Instance.SetCurrentProcessTheme(Theme.Dark);

            new Dark.Net.Wpf.SkinManager().RegisterSkins(
                lightThemeResources: new Uri("Themes/ColourfulLightTheme.xaml", UriKind.Relative),
                darkThemeResources: new Uri("Themes/ColourfulDarkTheme.xaml", UriKind.Relative));
        }
    }
}
