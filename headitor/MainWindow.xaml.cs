using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using RomStuff;

namespace headitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private fixer superfixer;
        private HeaderFixer headerfixer;

        public MainWindow()
        {
            InitializeComponent();
            ErrorMsg.Content = "";

            RomTypes.ItemsSource = fixer.getRomTypes();
            RomTypes.DisplayMemberPath = "Value";
            RomTypes.SelectedValuePath = "Key";
        }

        private void InputFileSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog filebox = new OpenFileDialog();
            filebox.Filter = "gb/gbc romz|*.gb;*.gbc";
            filebox.ShowDialog();

            InputFilename.Text = filebox.FileName;
            string origoutfilename = Regex.Replace(filebox.FileName, "\\.([^\\.]+)$", ".fix.${1}");
            string outfilename = origoutfilename;
            int appendNumber=0;
            while (System.IO.File.Exists(outfilename))
            {
                appendNumber++;
                outfilename = Regex.Replace(origoutfilename, "\\.fix\\.([^\\.]+)$",".fix"+appendNumber+".${1}");
            }
            OutputFilename.Text = outfilename;
            doLoad();
        }

        private void OutputFileSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog outfilebox = new OpenFileDialog();
            outfilebox.Filter = "gbc rom|*.gbc|gb rom|*.gb";
            outfilebox.CheckFileExists = false;
            outfilebox.ShowDialog();

            OutputFilename.Text = outfilebox.FileName;
        }


        private void LetsFuckingGo_Click(object sender, RoutedEventArgs e)
        {
            ErrorMsg.Content = "";
            try
            {
                if (superfixer == null || headerfixer == null)
                {
                    throw new Exception("no rom loaded");
                }

                byte? romtype = null;
                byte? ramsize = null;

                if (EnableHeaderType.IsChecked == true)
                {
                    romtype = Byte.Parse(RomType.Text, System.Globalization.NumberStyles.HexNumber);
                }

                if (EnableHeaderRamsize.IsChecked == true)
                {
                    ramsize = Byte.Parse(RamSize.Text, System.Globalization.NumberStyles.HexNumber);
                }

                headerfixer.headerFix((bool)EnableHeaderSize.IsChecked, (bool)EnableHeaderComp.IsChecked, (bool)EnableHeaderChecksum.IsChecked, romtype, ramsize);
                    
                headerfixer.save((bool)OpenEmu.IsChecked);
                ErrorMsg.Content = "★ " + "done!";
            }
            catch (Exception hmm)
            {
                ErrorMsg.Content = "★ " + hmm.Message;
            }

        }

        private void RomType_GotFocus(object sender, RoutedEventArgs e)
        {
            EnableHeaderType.IsChecked = true;
            RomTypes.SelectedValue = -1;
        }

        private void RamSize_GotFocus(object sender, RoutedEventArgs e)
        {
            EnableHeaderRamsize.IsChecked = true;
        }

        private void doLoad()
        {
            ErrorMsg.Content = "";
            try
            {
                superfixer = new fixer(InputFilename.Text);
                RomType.Text = superfixer.getCurrentRomType().ToString("X").PadLeft(2, '0');
                RamSize.Text = superfixer.getCurrentRamSize().ToString("X").PadLeft(2, '0');
                changeDropdownFromTextbox();
                RomType.Text = superfixer.getCurrentRomType().ToString("X").PadLeft(2, '0');
                RamSize.Text = superfixer.getCurrentRamSize().ToString("X").PadLeft(2, '0');
                headerfixer = new HeaderFixer(InputFilename.Text, OutputFilename.Text);
            }
            catch (Exception hmm)
            {
                ErrorMsg.Content = "★ " + hmm.Message;
            }
        }

        private void LoadButt_Click(object sender, RoutedEventArgs e)
        {
            doLoad();
        }

        private void RomTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RomTypes.SelectedValue != null )
            {
                int selectedint = int.Parse(RomTypes.SelectedValue.ToString());
                if (selectedint != -1)
                {
                    RomType.Text = selectedint.ToString("X").PadLeft(2, '0');
                }

            }

        }

        private void RomType_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void changeDropdownFromTextbox()
        {
            int result;
            bool idk = int.TryParse(RomType.Text, System.Globalization.NumberStyles.HexNumber, null, out result);
            if (idk && RomType.Text.Length == 2 && (fixer.getRomTypes().ContainsKey(result)))
            {
                RomTypes.SelectedValue = result;
            }
            else
            {
                RomTypes.SelectedValue = -1;
            }
        }

        private void RomTypes_GotFocus(object sender, RoutedEventArgs e)
        {
            EnableHeaderType.IsChecked = true;
        }

        private void InputFilename_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                doLoad();
            }
        }

    }
}
