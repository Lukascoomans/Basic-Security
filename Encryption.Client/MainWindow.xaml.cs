using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.AspNet.SignalR.Client;

namespace Encryption.Client
{
    
    public partial class MainWindow : Window
    {
        private IHubProxy chat;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var connection = new HubConnection("http://localhost:24127/");
            chat = connection.CreateHubProxy("chat");

            chat.On("newMessage", msg => { this.Dispatcher.Invoke(() => { MessagesBox.Items.Add(msg); }); });

            connection.Start().Wait();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            chat.Invoke<String>("SendMessage", InputBox.Text);
        }
    }
}
