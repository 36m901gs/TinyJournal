using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace TinyJournal
{
    internal class KeyManager
    {

        //Generate Key -- should this check for keys too? or just make it?
        
        public byte[] EncryptTest(string keypath)
        {


            //let's do a simple example first. then we'll do one where the key is made from the password

            byte[] encrypted;
            //dependency injection gonna be valuable here!
            using (Aes myAes = Aes.Create())
            {
                var keytest = myAes.Key; //alright so the key is here, gonna assume the IV is there too. So, two questions (1) save it? [answer:yes] (2) no others!
                ICryptoTransform encryptor = myAes.CreateEncryptor(myAes.Key, myAes.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.

                            //save the key (use for decrypt test)

                            //(1)step one - convert key to string (success)

                            //(2)step two, save it and use it to decrypt
                            var testKey = Convert.ToBase64String(myAes.Key);
                            var testIv = Convert.ToBase64String(myAes.IV);

                            swEncrypt.Write(testIv); //put it IV for later use

                            File.WriteAllBytes("C:\\Users\\njiso\\Desktop\\Key", myAes.Key);
                            File.WriteAllBytes("C:\\Users\\njiso\\Desktop\\IV", myAes.IV); // not necessary right? write this along with encrypted text and use it 
                            Console.WriteLine(testKey);
                            Console.WriteLine(testIv);
                        }

                    }
                    encrypted = msEncrypt.ToArray(); // will save later in another method

                }


            }

            return encrypted; // returns an encrypted array of bytes. this is your file to save (down the road. write now it only operates on "Hello WOrld"
            //(1) the question is #1 - how do I save the key and the IV here. ANd do you need the IV to decrypt? yes. so how do you save this
            //

        }

        //Encrypt File - On Saving. Have a specific file type? only need to run when you save
        public void EncryptSave()
        {
        }


        //Decrypt File - On Opening. Set the canvas equal to the decrypted file 

        public void DecryptOpen(string filepath)
        {

        }

        public static void DecryptFile(string inputFilePath, string outputFilePath, byte[] encryptionKey)
        {
            // Create a new AesManaged object to perform the encryption
            using (AesManaged aesManaged = new AesManaged())
            {
                // Set the encryption key
                aesManaged.Key = encryptionKey;

                // Create a decrytor to perform the stream transform
                ICryptoTransform decryptor = aesManaged.CreateDecryptor(aesManaged.Key, aesManaged.IV);

                // Open the encrypted file
                using (FileStream inputFileStream = new FileStream(inputFilePath, FileMode.Open))
                {
                    // Set the IV for the decrytor from the first 16 bytes of the file
                    aesManaged.IV = new byte[16];
                    inputFileStream.Read(aesManaged.IV, 0, 16);

                    // Create the output file
                    using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create))
                    {
                        // Create a CryptoStream to decrypt the data
                        using (CryptoStream cryptoStream = new CryptoStream(outputFileStream, decryptor, CryptoStreamMode.Write))
                        {
                            // Decrypt the data and write it to the output file
                            inputFileStream.CopyTo(cryptoStream);
                        }
                    }
                }
            }
        }



    }
}
