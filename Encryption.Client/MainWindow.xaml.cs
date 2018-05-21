using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Windows;
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

            if ( ! Directory.Exists("C:\\temp"))
            {
                Directory.CreateDirectory("C:\\temp");
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var connection = new HubConnection("http://localhost:24127/");
            chat = connection.CreateHubProxy("chat");

            chat.On("newMessage", msg => { this.Dispatcher.Invoke(() => { DecryptNewMessage(msg); }); });

            connection.Start().Wait();
        }

        private void DecryptNewMessage(byte[] message)
        {
            var dirPath = "C:\\temp";
            var pathFile1 = "C:\\temp\\file_1.txt";
            var pathFile2 = "C:\\temp\\file_2.txt";

            DirectoryInfo di = new DirectoryInfo(dirPath);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            File.WriteAllBytes(dirPath + "\\toSend.zip", message);
            ZipFile.ExtractToDirectory("C:\\temp\\toSend.zip", dirPath);

            Byte[] encryptedText;
            Byte[] key;
            
            using (StreamReader reader = File.OpenText(pathFile1))
            {
                char[] chars = reader.ReadLine().ToCharArray();
                encryptedText = new byte[chars.Length];
                for (int a = 0; a < chars.Length; a++)
                {
                    encryptedText[a] = (byte) chars[a];
                }
            }
            using (StreamReader reader = File.OpenText(pathFile2))
            {
                char[] chars = reader.ReadLine().ToCharArray();
                key = new byte[chars.Length];
                for (int a = 0; a < chars.Length; a++)
                {
                   key[a] = (byte)chars[a];
                }
            }

            string originalMessage = "Implement Encryption!";
            MessagesBox.Items.Add(originalMessage);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            String originalText = InputBox.Text;
            MessagesBox.Items.Add(originalText);

            byte[] key;
            byte[] IV;

            var pathFile1 = "C:\\temp\\file_1.txt";
            var pathFile2 = "C:\\temp\\file_1.txt";

            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                myRijndael.GenerateKey();
                myRijndael.GenerateIV();

                key = myRijndael.Key;
                IV = myRijndael.IV;

                Byte[] encrypted = AES.EncryptStringToBytes(originalText, key, IV);
                
                using (StreamWriter sw = File.CreateText(pathFile1))
                {
                    for (int i = 0; i < encrypted.Length; i++)
                    {
                        sw.Write(encrypted[i]);
                    }
                }
                using (StreamWriter sw = File.CreateText(pathFile2))
                {
                    for (int i = 0; i < encrypted.Length; i++)
                    {
                         sw.Write(key[i]); //zou nog geëncrypteerd moeten worden
                    }
                }
            }

           ZipFile.CreateFromDirectory("C:\\temp", "C:\\temp\\toSend.zip");

            byte[] zipBytes = File.ReadAllBytes("C:\\temp\\toSend.zip");
            chat.Invoke<Byte[]>("SendMessage", zipBytes);
            InputBox.Text = "";

            File.Delete(pathFile1);
            File.Delete(pathFile2);
            File.Delete("C:\\temp\\toSend.zip");
        }
    }
}
