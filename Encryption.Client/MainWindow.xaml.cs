using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
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
using encryption;
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

        private void DecryptNewMessage(byte[] message)
        {
            var pathFile1 = "C:\\temp\\file_1.txt";
            byte[] file1Bytes = message; //this code has to be replaced later
            File.WriteAllBytes(pathFile1, file1Bytes);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            String originalText = InputBox.Text;
            byte[] key;
            byte[] IV;

            var path = "C:\\temp\\file_1.txt";

            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {

                myRijndael.GenerateKey();
                myRijndael.GenerateIV();

                key = myRijndael.Key;
                IV = myRijndael.IV;

                Byte[] encrypted = AES.EncryptStringToBytes(originalText, key, IV);
                
                using (StreamWriter sw = File.CreateText(path))
                {
                    for (int i = 0; i < encrypted.Length; i++)
                    {
                        sw.Write(encrypted[i]);
                    }
                }
            }

           // chat.Invoke<String>("SendMessage", InputBox.Text);
            //InputBox.Text = "";

            File.Delete(path);
        }
    }
}
