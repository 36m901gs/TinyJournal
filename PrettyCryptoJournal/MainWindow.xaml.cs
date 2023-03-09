using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using System.Security.Cryptography;
using System.Data.Common;


namespace TinyJournal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*Application PRoperties*/

        bool current_state;
        string text_store; //for holding text during camo mode

        //Key Commands
        public static RoutedCommand ScrambleToggle = new RoutedCommand();
        KeyBinding kb = new KeyBinding(ScrambleToggle, new KeyGesture(Key.T, ModifierKeys.Control));





        public MainWindow()
        {

            // boot up functon
            var booter = new OnBoot();
            if(booter.AppBoot())
            {
                InitializeComponent();
            };
            textEditor.SyntaxHighlighting = null;

            textEditor.InputBindings.Add(kb);
            CommandBinding cb = new CommandBinding(ScrambleToggle, ScrambleToggleExecuted);
            textEditor.CommandBindings.Add(cb);
        }

        

        private void ScrambleToggleExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            scrambleBtn_Click(sender, e);
        }
   

        private void textEditor_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var visualLines = textEditor.Text;

            if (current_state)
            {
                if (current_state)
                {
                    Asterisk_Dropper(e);
  
                }
            }
        }


        private void Asterisk_Dropper(System.Windows.Input.KeyEventArgs e)
        {
            //(ADD) -- need to add functionality to manually add backspace

            //(1) store last non hidden value in text_store
            text_store += GetCharFromKey(e.Key);   //10-25 NEED TO MODIFY THIS TO ACTUALLY PUT THE CHAR AND NOT THE EVENT
            //(2) change it to an asterisk and add last typed value on text
            textEditor.Text += "*";
            textEditor.Select(textEditor.Text.Length, 0);
        }

        void openBtn_Click(object sender, RoutedEventArgs e)
        {

            var Opener = new FileSaving();
            textEditor.Text = Opener.DecryptTest();

        }
   
        void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Encrypt?", "", MessageBoxButtons.YesNo);

           if(dialogResult == System.Windows.Forms.DialogResult.Yes) {
               
                var Saver = new FileSaving();
                if (current_state) 
                {
                    Saver.EncryptTest(text_store); 
                }
                else{
                    
                    Saver.EncryptTest(textEditor.Text); 
                }

                this.Title = "Change Window TItle here";

            }

            else
            {
                Console.WriteLine("Test - normal saving. Will be later");

            }
  
        } 

        void KeyBtn_Click(object sender, RoutedEventArgs e)
        {

        }



        void scrambleBtn_Click(object sender, RoutedEventArgs e)
        {
            stateMaker();

            if (current_state == true) // toggle on
            {

                textEditor.IsReadOnly = true;
                text_store = textEditor.Text; // store all current text before we initialize everything
                textEditor.Text = String.Concat(Enumerable.Repeat("*", (textEditor.Text.Length)));
                textEditor.Select(textEditor.Text.Length, 0);


            }


            else // toggle off
            {
                //restore text - BROKEN
                textEditor.Text = text_store;
                textEditor.Select(textEditor.Text.Length, 0);
                text_store = "";
                textEditor.IsReadOnly = false;



            }


        }



        private void stateMaker()
        {
            if (current_state == true)
            {
                current_state = false;
            }
            else
            {
                current_state = true;
            }

        }

        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        public static char GetCharFromKey(Key key)
        {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }


    }
}
