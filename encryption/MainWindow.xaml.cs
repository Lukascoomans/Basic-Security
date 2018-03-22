using System;
using System.Collections.Generic;
using System.Linq;
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

namespace encryption
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            

                try
                {

                    string original = "Here is some data to encrypt!";

                    // Create a new instance of the RijndaelManaged 
                    // class.  This generates a new key and initialization  
                    // vector (IV). 
                    using (RijndaelManaged myRijndael = new RijndaelManaged())
                    {

                        myRijndael.GenerateKey();
                        myRijndael.GenerateIV();
                        // Encrypt the string to an array of bytes. 
                        byte[] encrypted = AES.EncryptStringToBytes(original, myRijndael.Key, myRijndael.IV);

                        Console.WriteLine(System.Text.Encoding.UTF8.GetString(encrypted));
                        // Decrypt the bytes to a string. 
                        string roundtrip = AES.DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);

                        //Display the original data and the decrypted data.
                        Console.WriteLine("Original:   {0}", original);
                        Console.WriteLine("Round Trip: {0}", roundtrip);
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e.Message);
                }



            try
            {
                //Create a UnicodeEncoder to convert between byte array and string.
                UnicodeEncoding ByteConverter = new UnicodeEncoding();

                //Create byte arrays to hold original, encrypted, and decrypted data.
                byte[] dataToEncrypt = ByteConverter.GetBytes("Data to Encrypt");
                byte[] encryptedData;
                byte[] decryptedData;

                //Create a new instance of RSACryptoServiceProvider to generate
                //public and private key data.
                using (RSACryptoServiceProvider RSAs = new RSACryptoServiceProvider())
                {

                    //Pass the data to ENCRYPT, the public key information 
                    //(using RSACryptoServiceProvider.ExportParameters(false),
                    //and a boolean flag specifying no OAEP padding.
                    encryptedData = RSA.RSAEncrypt(dataToEncrypt, RSAs.ExportParameters(false), false);

                    //Pass the data to DECRYPT, the private key information 
                    //(using RSACryptoServiceProvider.ExportParameters(true),
                    //and a boolean flag specifying no OAEP padding.
                    decryptedData = RSA.RSADecrypt(encryptedData, RSAs.ExportParameters(true), false);
                    

                    //Display the decrypted plaintext to the console. 
                    Console.WriteLine("Decrypted plaintext: {0}", ByteConverter.GetString(decryptedData));
                }
            }
            catch (ArgumentNullException)
            {
                //Catch this exception in case the encryption did
                //not succeed.
                Console.WriteLine("Encryption failed.");

            }

        }
    }
}
