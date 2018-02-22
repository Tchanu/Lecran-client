using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace Lecran
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket socket;
        public MainWindow()
        {
            socket = new Socket(this);
            socket.Init();
            InitializeComponent();
        }

        //Connect to server
        public void Connect(String machine_id)
        {
            MachineTextBox.Text = machine_id;
            MachineTextBox.Visibility = Visibility.Visible;
            MachineLabel.Visibility = Visibility.Visible;
            ConnectingToServerLabel.Visibility = Visibility.Hidden;
        }

        //Disconnect to server
        public void Disconnect()
        {
            MachineTextBox.Text = "";
            MachineTextBox.Visibility = Visibility.Hidden;
            MachineLabel.Visibility = Visibility.Hidden;
            ConnectingToServerLabel.Visibility = Visibility.Visible;
        }

        //Client Connect
        public void ClientConnect()
        {
            this.WindowState = (WindowState) FormWindowState.Minimized;
        }
    }
}

