using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dark.Net;
using PacketTool.Models;
using PacketTool.ViewModels;

namespace PacketTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DarkNet.Instance.SetWindowThemeWpf(this, Theme.Dark);
            DataContext = new MainWindowViewModel();
        }

        private void TreeViewItem_Click(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)sender;
            MainWindowModel.OnNodeClicked(treeViewItem);
        }
    }
}
