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
        private static string _userName;
        private static string _otherUser;
        private static bool _hasSendUsername = false;

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

            chat.On("SetUserName", userName => { this.Dispatcher.Invoke(() => { _otherUser = userName; }); });

            _userName = UsernametextBox.Text;

            connection.Start().Wait();

        }

        private void DecryptNewMessage(string msg)
        {
            var dirPath = "C:\\temp\\files";
            var pathFile1 = "C:\\temp\\files\\file_1.txt";
            var pathFile2 = "C:\\temp\\files\\file_2.txt";

            //recreate the zip and unzip it
            byte[] bytes = ConvertStringToArray(msg);
            File.WriteAllBytes("C:\\temp\\zips\\toSend.zip", bytes);
            ZipFile.ExtractToDirectory("C:\\temp\\zips\\toSend.zip", dirPath);

            Byte[] encryptedText;
            Byte[] key;
            Byte[] IV;
            
            using (StreamReader reader = File.OpenText(pathFile1))
            {
                string encryptedMessageString = reader.ReadLine();
                encryptedText = ConvertStringToArray(encryptedMessageString);
            }
            using (StreamReader reader = File.OpenText(pathFile2))
            {
                string keyString = reader.ReadLine();
                key = ConvertStringToArray(keyString);
                string ivString = reader.ReadLine();
                IV = ConvertStringToArray(ivString);
            }

            string originalMessage = AES.DecryptStringFromBytes(encryptedText, key, IV);
            MessagesBox.Items.Add( _otherUser + ": " + originalMessage);

            File.Delete(pathFile1);
            File.Delete(pathFile2);
            File.Delete("C:\\temp\\zips\\toSend.zip");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (! _hasSendUsername)
            {
                chat.Invoke<string>("SetUserName", _userName);
                _hasSendUsername = true;
            }

            String originalText = InputBox.Text;
            MessagesBox.Items.Add(_userName + ": " + originalText);

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
                    sw.WriteLine(ConvertArrayToString(encrypted));
                    sw.Close();
                }
                using (StreamWriter sw = File.CreateText(pathFile2))
                {
                    sw.WriteLine(ConvertArrayToString(key));
                    sw.WriteLine(ConvertArrayToString(IV));
                    sw.Close();
                }
            }

            ZipFile.CreateFromDirectory("C:\\temp\\files", "C:\\temp\\zips\\toSend.zip");

            byte[] zipBytes = File.ReadAllBytes("C:\\temp\\zips\\toSend.zip");

            string text = ConvertArrayToString(zipBytes);

            chat.Invoke<string>("SendMessage", text);
            InputBox.Text = "";

            File.Delete(pathFile1);
            File.Delete(pathFile2);
            File.Delete("C:\\temp\\zips\\toSend.zip");
        }

        private string ConvertArrayToString(Byte[] array)
        {
            string text = "";
            foreach (byte number in array)
            {
                text = text + number + ",";
            }

            text = text.Trim(',');
            return text;
        }

        private Byte[] ConvertStringToArray(String astring)
        {
            String[] strings = astring.Split(',');
            Byte[] bytes = new byte[strings.Length];

            for (int i = 0; i < strings.Length; i++)
            {
                bytes[i] = Convert.ToByte(strings[i]);
            }

            return bytes;
        }
    }
}
