using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
            if (!Directory.Exists("C:\\temp\\files"))
            {
                Directory.CreateDirectory("C:\\temp\\files");
            }
            if (!Directory.Exists("C:\\temp\\zips"))
            {
                Directory.CreateDirectory("C:\\temp\\zips");
            }
            
            DirectoryInfo di = new DirectoryInfo("C:\\temp\\zips");

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            DirectoryInfo directory = new DirectoryInfo("C:\\temp\\files");

            foreach (FileInfo file in directory.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directory.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var connection = new HubConnection("http://localhost:24127/");
            chat = connection.CreateHubProxy("chat");

            chat.On("newMessage", message => { this.Dispatcher.Invoke(() =>
            {
                var test = message;
                DecryptNewMessage(test);
            }); });

            connection.Start().Wait();
        }

        private void DecryptNewMessage(string msg)
        {
            var dirPath = "C:\\temp\\files";
            var pathFile1 = "C:\\temp\\files\\file_1.txt";
            var pathFile2 = "C:\\temp\\files\\file_2.txt";

            String[] strings = msg.Split(',');
            Byte[] bytes = new byte[strings.Length];

            for (int i = 0; i < strings.Length; i++)
            {
                bytes[i] = Convert.ToByte(strings[i]);
            }

            File.WriteAllBytes("C:\\temp\\zips\\toSend.zip", bytes);
            ZipFile.ExtractToDirectory("C:\\temp\\zips\\toSend.zip", dirPath);

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
            MessagesBox.Items.Add("Other person: " + originalMessage);

            File.Delete(pathFile1);
            File.Delete(pathFile2);
            File.Delete("C:\\temp\\zips\\toSend.zip");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            String originalText = InputBox.Text;
            MessagesBox.Items.Add("You: " + originalText);

            byte[] key;
            byte[] IV;

            var pathFile1 = "C:\\temp\\files\\file_1.txt";
            var pathFile2 = "C:\\temp\\files\\file_2.txt";

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
                    sw.Close();
                }
                using (StreamWriter sw = File.CreateText(pathFile2))
                {
                    for (int i = 0; i < encrypted.Length; i++)
                    {
                         sw.Write(key[i]); //zou nog geëncrypteerd moeten worden
                    }
                    sw.Close();
                }
            }

            ZipFile.CreateFromDirectory("C:\\temp\\files", "C:\\temp\\zips\\toSend.zip");

            byte[] zipBytes = File.ReadAllBytes("C:\\temp\\zips\\toSend.zip");

            string text = "";

            foreach (byte number in zipBytes)
            {
                text = text + number.ToString() + ",";
            }

            text = text.Trim(',');
            chat.Invoke<string>("SendMessage", text);
            InputBox.Text = "";

            File.Delete(pathFile1);
            File.Delete(pathFile2);
            File.Delete("C:\\temp\\zips\\toSend.zip");
        }
    }
}
