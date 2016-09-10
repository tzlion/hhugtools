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

namespace sintaxinator_win
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
                fixer superfixer = new fixer(InputFilename.Text, OutputFilename.Text);

                if (EnableBitFlip.IsChecked == true)
                {
                    if (BitFlipsAuto.IsChecked == true)
                    {
                        superfixer.flipBits(true);
                    }
                    else if (BitFlipsManual.IsChecked == true)
                    {
                        int parseResult;
                        int repeatCount = int.TryParse(FlipRepeat.Text, out parseResult) ? parseResult : 1;
                        superfixer.flipBits(false, ManualBits.Text, repeatCount);
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

                if (EnableHeaderFix.IsChecked == true)
                {
                    byte? romtype = null;
                    byte? ramsize = null;

                    if (EnableHeaderType.IsChecked == true)
                    {
                        romtype = Byte.Parse(RomType.Text);
                    }

                    if (EnableHeaderRamsize.IsChecked == true)
                    {
                        ramsize = Byte.Parse(RamSize.Text);
                    }

                    superfixer.headerFix((bool)EnableHeaderSize.IsChecked, (bool)EnableHeaderComp.IsChecked, (bool)EnableHeaderChecksum.IsChecked, romtype, ramsize);
                    
                }
                superfixer.save((bool)OpenEmu.IsChecked);
            }
            catch (Exception hmm)
            {
                ErrorMsg.Content = "★ " + hmm.Message;
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

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                fixer superfixer = new fixer(InputFilename.Text, OutputFilename.Text);

                superfixer.bankChecksums();



            }
            catch (Exception hmm)
            {
                ErrorMsg.Content = "★ " + hmm.Message;
            }
        }

        private void testswap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                fixer superfixer = new fixer(InputFilename.Text, OutputFilename.Text);

                superfixer.testswap();

                superfixer.save((bool)OpenEmu.IsChecked);



            }
            catch (Exception hmm)
            {
                ErrorMsg.Content = "★ " + hmm.Message;
            }


        }
    }
}
