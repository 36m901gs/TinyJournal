using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ICSharpCode.AvalonEdit;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TinyJournal
{
    internal class FileSaving
    {
        //file directory
        string journalPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Cryptojournal" + "\\journals";

        const int keySize = 64;
        const int iterations = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;
        public void EncryptTest(string inputText)
        {
            //--------------File Dialogue------------------------

            var filePath = string.Empty;
            System.Windows.Forms.SaveFileDialog saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();

            saveFileDialog1.Filter = "secret files (*.shh)|*.shh";
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.InitialDirectory = journalPath;
            saveFileDialog1.RestoreDirectory = true;

            if (!Directory.Exists(journalPath))
            {
                DirectoryInfo di = Directory.CreateDirectory(journalPath);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string outFile = saveFileDialog1.FileName; 
                EncryptFile(outFile, inputText);
            }            
        }

        public void EncryptFile(string outFile, string inputText)
        {
            Aes aes = Aes.Create();
            ICryptoTransform encryptor = aes.CreateEncryptor();

            int l_AesIv = aes.IV.Length;
            byte[] LenIV = BitConverter.GetBytes(l_AesIv);
            int l_Key = aes.Key.Length;
            byte[] LenKey = BitConverter.GetBytes(l_Key);

            using (var outFs = new FileStream(outFile, FileMode.Create))
            {
                //header file
                outFs.Write(LenKey, 0, 4);
                outFs.Write(LenIV, 0, 4);
                outFs.Write(aes.Key, 0, l_Key);
                outFs.Write(aes.IV, 0, l_AesIv);



                // encrypting, and saving that ish
                using (var outStreamEncrypted = new CryptoStream(outFs, encryptor, CryptoStreamMode.Write))
                {

                    using (StreamWriter swEncrypt = new StreamWriter(outStreamEncrypted))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(inputText);
                    }
                }
            }

        }

        public string DecryptTest()
        {

            var filePath = string.Empty;

            using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.InitialDirectory = journalPath;
                openFileDialog.Filter = "secret files (*.shh)|*.shh";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    return DecryptFile(filePath);

                }
                else{
                    return string.Empty;

                }
            }


        }

        string DecryptFile(string filePath)
        {

            Aes aes = Aes.Create();

            byte[] LenK = new byte[4];
            byte[] LenIV = new byte[4];
            using (var openedSave = new FileStream(filePath, FileMode.Open))
            {

                openedSave.Seek(0, SeekOrigin.Begin);
                openedSave.Read(LenK, 0, 3);
                openedSave.Seek(4, SeekOrigin.Begin);
                openedSave.Read(LenIV, 0, 3);


                // ------------------------------------------------------------->

                int lenK = BitConverter.ToInt32(LenK, 0);
                int lenIV = BitConverter.ToInt32(LenIV, 0);

                Console.WriteLine(lenK);
                Console.WriteLine(lenIV);

                byte[] Key_oof = new byte[lenK];
                byte[] IV = new byte[lenIV];

                openedSave.Seek(8, SeekOrigin.Begin);
                openedSave.Read(Key_oof, 0, lenK);
                openedSave.Seek(8 + lenK, SeekOrigin.Begin);
                openedSave.Read(IV, 0, lenIV);

                string bitString = BitConverter.ToString(Key_oof);
                string bitIV = BitConverter.ToString(IV);



                Console.WriteLine(bitString);
                Console.WriteLine(bitIV);

                int startC = lenK + lenIV + 8;
                int lenC = (int)openedSave.Length - startC;
                byte[] data = new byte[16];


                openedSave.Seek(startC, SeekOrigin.Begin);
                openedSave.Read(data, 0, 16);

                aes.IV = IV;
                aes.Key = Key_oof;

                //decrypt here
                using (var decryptor = aes.CreateDecryptor())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        // create a crypto stream to decrypt the data
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                        {
                            // read the encrypted data in chunks
                            int chunkSize = 1024;
                            byte[] chunk = new byte[chunkSize];
                            int bytesRead;

                            openedSave.Seek(startC, SeekOrigin.Begin);
                            while ((bytesRead = openedSave.Read(chunk, 0, chunkSize)) > 0)
                            {
                                // write the encrypted data to the crypto stream
                                cs.Write(chunk, 0, bytesRead);
                            }
                            // write the encrypted data to the crypto stream
                        }

                        // get the decrypted data as a byte array
                        var final_result = ms.ToArray();
                        string un_encrypted = Encoding.UTF8.GetString(final_result); //problem the entire time was wrong encoding lol

                        var pre_result = Convert.ToBase64String(data);

                        Debug.WriteLine("Unencrypted final result is ");
                        Debug.WriteLine(un_encrypted);
                        Debug.WriteLine("Encryped result we're getting is");
                        Debug.WriteLine(pre_result);

                        //Assign the text field 
                        return un_encrypted;

                    }
                }
            }

        }

        bool VerifyPassword(string password, string hash, byte[] salt)
        {
            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);
            var generaltest = Convert.ToHexString(hashToCompare);
            var booltest = hashToCompare.SequenceEqual(Convert.FromHexString(hash));
            Console.WriteLine(hash);
            return booltest;
        }
    

    }
}
