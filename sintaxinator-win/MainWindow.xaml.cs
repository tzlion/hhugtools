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

namespace Sintaxinator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ErrorMsg.Content = "";
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
            try
            {
                SintaxFixer superfixer = new SintaxFixer(InputFilename.Text, OutputFilename.Text);
                if (EnableBitFlip.IsChecked == true)
                {
                    if (BitFlipsAuto.IsChecked == true)
                    {
                        superfixer.flipBits(true);
                    }
                    else if (BitFlipsManual.IsChecked == true)
                    {
                        superfixer.flipBits(false, ManualBits.Text, int.Parse(FlipRepeat.Text));
                    }
                }
                if (EnableReorder.IsChecked == true)
                {
                    if (ReorderAuto.IsChecked == true)
                    {
                        superfixer.reorder(false);
                    }
                    else if (ReorderBankNo.IsChecked == true)
                    {
                        superfixer.reorder(true);
                    }
                }
                superfixer.save();

                if (EnableHeaderFix.IsChecked == true)
                {
                    HeaderFixer headerfixer = new HeaderFixer(OutputFilename.Text, OutputFilename.Text);
                    headerfixer.headerFix(
                        (bool)EnableHeaderSize.IsChecked, 
                        (bool)EnableHeaderComp.IsChecked, 
                        (bool)EnableHeaderChecksum.IsChecked,
                        (bool)EnableHeaderType.IsChecked ? Byte.Parse(RomType.Text, System.Globalization.NumberStyles.HexNumber) : (byte?)null,
                        (bool)EnableHeaderRamsize.IsChecked ? Byte.Parse(RamSize.Text, System.Globalization.NumberStyles.HexNumber) : (byte?)null
                    );
                    headerfixer.save();
                }

                if (OpenEmu.IsChecked == true)
                {
                    System.Diagnostics.Process.Start(OutputFilename.Text);
                }
            }
            catch (Exception hmm)
            {
                populateErrorMessage(hmm);
            }

        }

        private void BankCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //HashComputer hashcom = new HashComputer(InputFilename.Text, InputFilename.Text + ".txt");
                //hashcom.bankChecksums();
                OverdumpDetector od = new OverdumpDetector(InputFilename.Text);
                MessageBox.Show(od.getSizeInfo());
            }
            catch (Exception hmm)
            {
                populateErrorMessage(hmm);
            }
        }

        private void TestSwap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DigiSapphTestThing testThing = new DigiSapphTestThing(InputFilename.Text, OutputFilename.Text);
                testThing.testswap();
                testThing.save((bool)OpenEmu.IsChecked);
            }
            catch (Exception hmm)
            {
                populateErrorMessage(hmm);
            }
        }

        private void ManualBits_GotFocus(object sender, RoutedEventArgs e)
        {
            BitFlipsManual.IsChecked = true;
        }

        private void RomType_GotFocus(object sender, RoutedEventArgs e)
        {
            EnableHeaderType.IsChecked = true;
        }

        private void RamSize_GotFocus(object sender, RoutedEventArgs e)
        {
            EnableHeaderRamsize.IsChecked = true;
        }

        private void populateErrorMessage(Exception e)
        {
            ErrorMsg.Content = "★ " + e.Message;
        }

    }
}
