using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Input;
using System.Text.RegularExpressions;
using Common.Utility;
using Common.Rom;

namespace headitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HeaderReader headerReader;

        public MainWindow()
        {
            InitializeComponent();
            ErrorMsg.Content = "";

            RomTypes.ItemsSource = HeaderReader.GetRomTypes();
            RomTypes.DisplayMemberPath = "Value";
            RomTypes.SelectedValuePath = "Key";
        }

        private void InputFileSelect_Click(object sender, RoutedEventArgs e)
        {
            String inputFilename = FileSelection.SelectInputFile();
            InputFilename.Text = inputFilename;
            OutputFilename.Text = FileSelection.DetermineOutputFilename(inputFilename);
            doLoad();
        }

        private void OutputFileSelect_Click(object sender, RoutedEventArgs e)
        {
            OutputFilename.Text = FileSelection.SelectOutputFile();
        }
    
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMsg.Content = "";
            try
            {
                if (headerReader == null)
                {
                    throw new Exception("no rom loaded");
                }

                HeaderFixer headerfixer = new HeaderFixer(InputFilename.Text, OutputFilename.Text);

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

                headerfixer.HeaderFix((bool)EnableHeaderSize.IsChecked, (bool)EnableHeaderComp.IsChecked, (bool)EnableHeaderChecksum.IsChecked, romtype, ramsize);
                    
                headerfixer.Save((bool)OpenEmu.IsChecked);
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
                headerReader = new HeaderReader(InputFilename.Text);
                RomType.Text = headerReader.GetCurrentRomType().ToString("X").PadLeft(2, '0');
                RamSize.Text = headerReader.GetCurrentRamSize().ToString("X").PadLeft(2, '0');
                changeDropdownFromTextbox();
                RomType.Text = headerReader.GetCurrentRomType().ToString("X").PadLeft(2, '0');
                RamSize.Text = headerReader.GetCurrentRamSize().ToString("X").PadLeft(2, '0');
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
            if (idk && RomType.Text.Length == 2 && (HeaderReader.GetRomTypes().ContainsKey(result)))
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
