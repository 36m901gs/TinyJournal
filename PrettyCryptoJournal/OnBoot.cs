using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.VisualBasic;
using System.Security.Cryptography;
using System.DirectoryServices;
using System.Windows.Shapes;

namespace TinyJournal
{
    internal class OnBoot
    {

        //location of directories
        string AppDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Cryptojournal";
        string userStuff = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Cryptojournal" + "\\.deb";


        //generate password hash hash function 

        const int keySize = 64;
        const int iterations = 350000;
        HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

        void HashPasword(string password, out byte[] salt)
        {
            salt = RandomNumberGenerator.GetBytes(keySize);

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                hashAlgorithm,
                keySize);

            var hashSave = Convert.ToHexString(hash);

            //Does the directory exist? IF not make it?
            if (!Directory.Exists(userStuff))
            {
                DirectoryInfo di = Directory.CreateDirectory(userStuff);
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            File.WriteAllText(userStuff + "\\test", hashSave); 
            File.WriteAllBytes(userStuff + "\\salt", salt);
        }

        //verify password
        bool VerifyPassword(string password, string hash, byte[] salt)
        {
            var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keySize);
            var generaltest = Convert.ToHexString(hashToCompare);
            var booltest = hashToCompare.SequenceEqual(Convert.FromHexString(hash));
            Console.WriteLine(hash);
            return booltest;
        }


        public bool Xml()
        {

         if(File.Exists(userStuff + "\\test") && File.Exists(userStuff + "\\salt"))
           {
                return true;
            }
            return false;

        }

        public bool PasswordRules(string password)
        {
            if (password.Length > 4)
            {
                return true;
            }
            else
            {
                return false;
            }

            

        }


        public bool GenerateNewXML()
        {

            //make dialogue box asking for password -- will later figure out how to use this hash in a key?
            //wrap this up in a function later

            string password ="";
            string password2 = "";

            while(!PasswordRules(password)) 
            {
                password = Microsoft.VisualBasic.Interaction.InputBox("Password", "Security", "Please Type in A Password");
            }

            while(password != password2 && PasswordRules(password2)==false)
            {
                password2 = Microsoft.VisualBasic.Interaction.InputBox("Confirm Password", "Security", "Please Retype Password");
            }

            //use input to create password and salt
            HashPasword(password2, out var salt);


            return true;
        }


        //user access - noticing, generating, authenticating users -- will have to make from scratch to avoid using sql db
        public bool AppBoot()
        {
            string userinput = "";

            if(!Directory.Exists(AppDirectory))      //directory doesnt exist? make it)
            {
                System.IO.Directory.CreateDirectory(AppDirectory);
                //make folder for password stuff. Hidden!
                System.IO.Directory.CreateDirectory(userStuff);
                File.SetAttributes(userStuff, File.GetAttributes(userStuff) | FileAttributes.Hidden);
            }

            while (!Xml())
            {
                GenerateNewXML();
            }

            //password compare 
            while(!(VerifyPassword(userinput, File.ReadAllText(userStuff + "\\test"),File.ReadAllBytes(userStuff + "\\salt"))))
            {
                //once this works, add a count to it
                userinput = Microsoft.VisualBasic.Interaction.InputBox("Password", "Security", "Please Type in A Password");

            };

            return true;


        }
            
    }
}
