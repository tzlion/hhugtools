using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Common.Utility;

namespace Sintaxinator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool bbdMode;
        
        public MainWindow()
        {
            InitializeComponent();
            ErrorMsg.Content = "";
        }

        private void InputFileSelect_Click(object sender, RoutedEventArgs e)
        {
            string inputFilename = FileSelection.SelectInputFile();
            InputFilename.Text = inputFilename;
            OutputFilename.Text = FileSelection.DetermineOutputFilename(inputFilename);
        }

        private void OutputFileSelect_Click(object sender, RoutedEventArgs e)
        {
            OutputFilename.Text = FileSelection.SelectOutputFile();
        }

        private byte ParseInputAsByte(TextBox input)
        {
            return byte.Parse(input.Text, System.Globalization.NumberStyles.HexNumber);
        }

        private void DoTheThing(object sender, RoutedEventArgs e)
        {
            try
            {
                RomProcessor romProcessor = new RomProcessor();
                
                romProcessor.CopyRom(InputFilename.Text, OutputFilename.Text);

                if (bbdMode)
                {
                    if (EnableFullAuto.IsChecked == true)
                    {
                        romProcessor.ProcessBbdFullAuto(OutputFilename.Text, ParseInputAsByte(BitScrambleMode), 
                            ParseInputAsByte(ReorderMode));
                    }
                    else
                    {
                        romProcessor.ProcessBbd(OutputFilename.Text, EnableBBDDescramble.IsChecked ?? false,
                            BBDBitDescramble.Text, EnableReorder.IsChecked ?? false, ReorderAuto.IsChecked ?? false,
                            ParseInputAsByte(ReorderAutoMode), ReorderBankNo.IsChecked ?? false,
                            ReorderSpecified.IsChecked ?? false, ReorderSpecifiedOrder.Text);
                    }
                }
                else
                {
                    if (EnableFullAuto.IsChecked == true)
                    {
                        byte[] xors =
                        {
                            ParseInputAsByte(ManualBits1),
                            ParseInputAsByte(ManualBits2),
                            ParseInputAsByte(ManualBits3),
                            ParseInputAsByte(ManualBits4)
                        };
                        romProcessor.ProcessSintaxFullAuto(OutputFilename.Text, ParseInputAsByte(ReorderMode), xors);
                    }
                    else
                    {
                        romProcessor.ProcessSintax(OutputFilename.Text, EnableReorder.IsChecked ?? false,
                            ReorderAuto.IsChecked ?? false, ParseInputAsByte(ReorderAutoMode),
                            ReorderBankNo.IsChecked ?? false, ReorderSpecified.IsChecked ?? false,
                            ReorderSpecifiedOrder.Text, EnableXor.IsChecked ?? false, ManualBits.Text,
                            XorRepeat.Text);   
                    }
                }

                if (EnableHeaderFix.IsChecked == true)
                {
                    romProcessor.FixHeader(OutputFilename.Text, EnableHeaderSize.IsChecked ?? false,
                        EnableHeaderComp.IsChecked ?? false, EnableHeaderChecksum.IsChecked ?? false,
                        EnableHeaderType.IsChecked ?? false, EnableHeaderRamsize.IsChecked ?? false,
                        ParseInputAsByte(RomType), ParseInputAsByte(RamSize));
                }

                if (OpenEmu.IsChecked == true)
                {
                    System.Diagnostics.Process.Start(OutputFilename.Text);
                }
            }
            catch (Exception hmm)
            {
                PopulateErrorMessage(hmm);
            }

        }

        private void CheckEnableHeaderType(object sender, RoutedEventArgs e)
        {
            EnableHeaderType.IsChecked = true;
        }

        private void CheckEnableRamSize(object sender, RoutedEventArgs e)
        {
            EnableHeaderRamsize.IsChecked = true;
        }

        private void PopulateErrorMessage(Exception e)
        {
            ErrorMsg.Content = "★ " + e.Message;
        }

        private void ChangeMode(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!bbdMode)
                {
                    OperationsHeader.Content = "BBD mode";
                    bbdMode = true;
                    XorControls.Visibility = Visibility.Hidden;
                    BBDStuff.Visibility = Visibility.Visible;
                    BitScrambleLabel.Visibility = Visibility.Visible;
                    BitScrambleMode.Visibility = Visibility.Visible;
                    XORsLabel.Visibility = Visibility.Hidden;
                    ManualBits1.Visibility = Visibility.Hidden;
                    ManualBits2.Visibility = Visibility.Hidden;
                    ManualBits3.Visibility = Visibility.Hidden;
                    ManualBits4.Visibility = Visibility.Hidden;
                }
                else
                {
                    OperationsHeader.Content = "Sintax mode";
                    bbdMode = false;
                    XorControls.Visibility = Visibility.Visible;
                    BBDStuff.Visibility = Visibility.Hidden;
                    BitScrambleLabel.Visibility = Visibility.Hidden;
                    BitScrambleMode.Visibility = Visibility.Hidden;
                    XORsLabel.Visibility = Visibility.Visible;
                    ManualBits1.Visibility = Visibility.Visible;
                    ManualBits2.Visibility = Visibility.Visible;
                    ManualBits3.Visibility = Visibility.Visible;
                    ManualBits4.Visibility = Visibility.Visible;
                }

            }
            catch (Exception hmm)
            {
                PopulateErrorMessage(hmm);
            }
        }

        private void UnsetNonAuto(object sender, RoutedEventArgs e)
        {
            // this event, apparently, is fired before the other 2 checkboxes have even loaded
            // so gotta check if they exist first
            if (EnableReorder != null) EnableReorder.IsChecked = false;
            if (EnableXor != null) EnableXor.IsChecked = false;
            if (EnableBBDDescramble != null) EnableBBDDescramble.IsChecked = false;
        }
        
        private void UnsetAuto(object sender, RoutedEventArgs e)
        {
            if (EnableFullAuto != null) EnableFullAuto.IsChecked = false;
        }

    }
}
