using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
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
            if ( ! Directory.Exists("C:\\secret"))
            {
                Directory.CreateDirectory("C:\\secret");
            }
            if (!Directory.Exists("C:\\temp\\files"))
            {
                Directory.CreateDirectory("C:\\temp\\files");
            }
            if (!Directory.Exists("C:\\secret\\files"))
            {
                Directory.CreateDirectory("C:\\secret\\files");
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

            chat.On("namesConfig", message => { this.Dispatcher.Invoke(() =>
            {
                var test = message;
                setOtherName(test);
            }); });
            
            _userName = UsernametextBox.Text;

            

            RSACryptoServiceProvider user = new RSACryptoServiceProvider();

            var rsaParameters = user.ExportParameters(true);
            var ownPrivateKeyPath= "C:\\secret\\files\\privatekey_"+_userName+".txt";
            var ownPublicKeyPath= "C:\\secret\\files\\publickey_"+_userName+".txt";

            using (StreamWriter sw = File.CreateText(ownPrivateKeyPath))
            {
                sw.WriteLine(ConvertArrayToString(rsaParameters.Modulus));
                sw.WriteLine(ConvertArrayToString(rsaParameters.Exponent));
                sw.WriteLine(ConvertArrayToString(rsaParameters.D));
                sw.WriteLine(ConvertArrayToString(rsaParameters.DP));
                sw.WriteLine(ConvertArrayToString(rsaParameters.DQ));
                sw.WriteLine(ConvertArrayToString(rsaParameters.P));
                sw.WriteLine(ConvertArrayToString(rsaParameters.Q));
                sw.WriteLine(ConvertArrayToString(rsaParameters.InverseQ));
                sw.Close();
            }

            using (StreamWriter sw = File.CreateText(ownPublicKeyPath))
            {
                sw.WriteLine(ConvertArrayToString(rsaParameters.Modulus));
                sw.WriteLine(ConvertArrayToString(rsaParameters.Exponent));
                sw.Close();
            }


            connection.Start().Wait();

            chat.Invoke<string>("namesConfig", _userName);
            ConnectionStatuslabel.Content = "Connected";
            ConnectionStatuslabel.Foreground = new SolidColorBrush(Colors.Green);
        }

        private void setOtherName(string msg)
        {
            if (_otherUser == null)
            {
                chat.Invoke<string>("namesConfig", _userName);
            }

            _otherUser = msg;
        }
        
        private void DecryptNewMessage(string msg)
        {


            var dirPath = "C:\\temp\\files";
            var pathFile1 = "C:\\temp\\files\\file_1.txt";
            var pathFile2 = "C:\\temp\\files\\file_2.txt";
            var pathFile3 = "C:\\temp\\files\\file_3.txt";
            var ownprivatekeypath = "C:\\secret\\files\\privatekey_"+_userName+".txt";
            var otherPublickeypath = "C:\\secret\\files\\publickey_"+_otherUser+".txt";

            //recreate the zip and unzip it
            byte[] bytes = ConvertStringToArray(msg);
            File.WriteAllBytes("C:\\temp\\zips\\toSend.zip", bytes);
            ZipFile.ExtractToDirectory("C:\\temp\\zips\\toSend.zip", dirPath);

            Byte[] encryptedText;
            Byte[] key;
            Byte[] IV;
            Byte[] encryptedHashOfOriginalText;
            
            
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
            using (StreamReader reader = File.OpenText(pathFile3))
            {
                encryptedHashOfOriginalText = ConvertStringToArray(reader.ReadLine());
            }

            //read recipient public key parameters
            RSACryptoServiceProvider otherUser = new RSACryptoServiceProvider();
            RSAParameters otherRsaParameters = new RSAParameters();
            using (StreamReader reader = File.OpenText(otherPublickeypath))
            {
                otherRsaParameters.Modulus = ConvertStringToArray(reader.ReadLine());
                otherRsaParameters.Exponent = ConvertStringToArray(reader.ReadLine());

            }
            otherUser.ImportParameters(otherRsaParameters);


            //read own private key parameters
            RSACryptoServiceProvider ownUser = new RSACryptoServiceProvider();
            RSAParameters ownRsaParameters = new RSAParameters();
            using (StreamReader reader = File.OpenText(ownprivatekeypath))
            {
                ownRsaParameters.Modulus = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.Exponent = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.D = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.DP = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.DQ = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.P = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.Q = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.InverseQ = ConvertStringToArray(reader.ReadLine());

            }
            ownUser.ImportParameters(ownRsaParameters);

            //decrypt using own private key
            IV =RSAencryptor.RSADecrypt(IV, ownUser.ExportParameters(true), true);
            key =RSAencryptor.RSADecrypt(key, ownUser.ExportParameters(true), true);


            var hashofFile = RSAencryptor.RSADecrypt(encryptedHashOfOriginalText, otherUser.ExportParameters(false), true);

            


            string originalMessage = AES.DecryptStringFromBytes(encryptedText, key, IV);

           
            byte[] hashedBytes;
            using (var sha256 = SHA256.Create())
            {
                hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(originalMessage));


            }

            if(RSAencryptor.VerifySignedHash(hashedBytes, encryptedHashOfOriginalText, otherUser.ExportParameters(false)))
            {
                Console.WriteLine("hash is the same");
            }
            else
            {
                Console.WriteLine("hash is not the same");
            }



            MessagesBox.Items.Add( _otherUser + ": " + originalMessage);

            File.Delete(pathFile1);
            File.Delete(pathFile2);
            File.Delete(pathFile3);
            File.Delete("C:\\temp\\zips\\toSend.zip");
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            String originalText = InputBox.Text;
            MessagesBox.Items.Add(_userName + ": " + originalText);

            byte[] key;
            byte[] IV;

            var pathFile1 = "C:\\temp\\files\\file_1.txt";
            var pathFile2 = "C:\\temp\\files\\file_2.txt";
            var pathFile3 = "C:\\temp\\files\\file_3.txt";
            var otherPubicKey = "C:\\secret\\files\\publickey_" + _otherUser + ".txt";
            var ownPrivateKey = "C:\\secret\\files\\privatekey_" + _userName + ".txt";

            //generates AES keys
            Byte[] encrypted;
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                myRijndael.GenerateKey();
                myRijndael.GenerateIV();
                key = myRijndael.Key;
                IV = myRijndael.IV;

                encrypted = AES.EncryptStringToBytes(originalText, key, IV);
            }

            //read recipient public key parameters
            RSACryptoServiceProvider otherUser = new RSACryptoServiceProvider();
            RSAParameters otherRsaParameters = new RSAParameters();
            using (StreamReader reader = File.OpenText(otherPubicKey))
            {
                otherRsaParameters.Modulus = ConvertStringToArray(reader.ReadLine());
                otherRsaParameters.Exponent = ConvertStringToArray(reader.ReadLine());

            }
            otherUser.ImportParameters(otherRsaParameters);

            //read own private key parameters
            RSACryptoServiceProvider ownUser = new RSACryptoServiceProvider();
            RSAParameters ownRsaParameters = new RSAParameters();
            using (StreamReader reader = File.OpenText(ownPrivateKey))
            {
                ownRsaParameters.Modulus = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.Exponent = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.D = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.DP = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.DQ = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.P = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.Q = ConvertStringToArray(reader.ReadLine());
                ownRsaParameters.InverseQ = ConvertStringToArray(reader.ReadLine());

            }
            ownUser.ImportParameters(ownRsaParameters);


            var encryptedKey = RSAencryptor.RSAEncrypt(key, otherUser.ExportParameters(false), true);
            var encryptedIV = RSAencryptor.RSAEncrypt(IV, otherUser.ExportParameters(false), true);

            byte[] encryptedHashOfOriginalText;
            using (var sha256 = SHA256.Create())
            {
               var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(originalText));

                encryptedHashOfOriginalText = RSAencryptor.HashAndSignBytes(hashedBytes, ownUser.ExportParameters(true));


            }
            //write data to files
            using (StreamWriter sw = File.CreateText(pathFile1))
            {
                sw.WriteLine(ConvertArrayToString(encrypted));
                sw.Close();
            }
            using (StreamWriter sw = File.CreateText(pathFile2))
            {
                sw.WriteLine(ConvertArrayToString(encryptedKey));
                sw.WriteLine(ConvertArrayToString(encryptedIV));
                sw.Close();
            }
            using (StreamWriter sw = File.CreateText(pathFile3))
            {

                sw.WriteLine(ConvertArrayToString(encryptedHashOfOriginalText));
                sw.Close();
            }

            //zip files to send
            ZipFile.CreateFromDirectory("C:\\temp\\files", "C:\\temp\\zips\\toSend.zip");

            byte[] zipBytes = File.ReadAllBytes("C:\\temp\\zips\\toSend.zip");

            string text =ConvertArrayToString(zipBytes);

            
            InputBox.Text = "";

            File.Delete(pathFile1);
            File.Delete(pathFile2);
            File.Delete(pathFile3);
            File.Delete("C:\\temp\\zips\\toSend.zip");
            chat.Invoke<string>("SendMessage", text);
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
