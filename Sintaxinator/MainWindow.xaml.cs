using System;
using System.Windows;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using Sintaxinator.Fixers;
using Common.Utility;
using Common.Rom;

namespace Sintaxinator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool bbdMode = false;
        
        public MainWindow()
        {
            InitializeComponent();
            ErrorMsg.Content = "";
        }

        private void InputFileSelect_Click(object sender, RoutedEventArgs e)
        {
            String inputFilename = FileSelection.SelectInputFile();
            InputFilename.Text = inputFilename;
            OutputFilename.Text = FileSelection.DetermineOutputFilename(inputFilename);
        }

        private void OutputFileSelect_Click(object sender, RoutedEventArgs e)
        {
            OutputFilename.Text = FileSelection.SelectOutputFile();
        }

        private byte[] parseReorderingString(String input)
        {
            char[] reorderingChars = input.ToCharArray();
            if (reorderingChars.Length != 8)
            {
                throw new Exception("Reordering must be 8 digits");
            }

            byte[] reordering = new byte[8];
            for (int x = 0; x < 8; x++)
            {
                reordering[x] = byte.Parse(reorderingChars[x].ToString());
            }

            return reordering;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (bbdMode)
                {
                    if (EnableFullAuto.IsChecked == true)
                    {
                        throw new Exception("Full auto not supported for BBD yet");
                    }
                    
                    BBDFixer bbdFixer = new BBDFixer(InputFilename.Text, OutputFilename.Text);
                    if (EnableBBDDescramble.IsChecked == true)
                    {
                        byte[] reordering = parseReorderingString(BBDBitDescramble.Text);                        
                        bbdFixer.ReorderAllBytes(reordering);
                        bbdFixer.Save();
                    }

                    if (EnableReorder.IsChecked == true)
                    {
                        SintaxFixer sintaxFixer = new SintaxFixer(OutputFilename.Text, OutputFilename.Text);
                        if (ReorderAuto.IsChecked == true)
                        {
                            throw new Exception("Reorder mode not supported for BBD yet");
                            byte reorderMode = byte.Parse(ReorderAutoMode.Text, System.Globalization.NumberStyles.HexNumber);
                            sintaxFixer.reorder(false, sintaxFixer.getSintaxBankReorderings(reorderMode));
                        }
                        else if (ReorderBankNo.IsChecked == true)
                        {
                            sintaxFixer.reorder(true);
                        }
                        else if (ReorderSpecified.IsChecked == true)
                        {
                            byte[] reordering = parseReorderingString(ReorderSpecifiedOrder.Text);            
                            sintaxFixer.reorder(false, reordering);
                        }
                        sintaxFixer.Save();
                    }
                }
                else
                {
                    SintaxFixer sintaxFixer = new SintaxFixer(InputFilename.Text, OutputFilename.Text);

                    if (EnableFullAuto.IsChecked == true)
                    {
                        byte reorderMode = byte.Parse(ReorderMode.Text, System.Globalization.NumberStyles.HexNumber);
                        sintaxFixer.reorder(false, sintaxFixer.getSintaxBankReorderings(reorderMode));
                        string[] flipstrings = {  
                            "0x" + ManualBits1.Text, 
                            "0x" + ManualBits2.Text, 
                            "0x" + ManualBits3.Text, 
                            "0x" + ManualBits4.Text
                        };
                        byte[] manualXors = parseFlipStringsToXors(flipstrings);
                        sintaxFixer.xorAllData(false, manualXors, 64);
                    }
                    else
                    {
                        if (EnableXor.IsChecked == true)
                        {
                            string[] flipstrings = ManualBits.Text.Split(new String[] { "|" }, new StringSplitOptions()); ;
                            byte[] manualXors = parseFlipStringsToXors(flipstrings);
                            sintaxFixer.xorAllData(false, manualXors, int.Parse(XorRepeat.Text));
                        }
                        if (EnableReorder.IsChecked == true)
                        {
                            if (ReorderAuto.IsChecked == true)
                            {
                                byte reorderMode = byte.Parse(ReorderAutoMode.Text, System.Globalization.NumberStyles.HexNumber);
                                sintaxFixer.reorder(false, sintaxFixer.getSintaxBankReorderings(reorderMode));
                            }
                            else if (ReorderBankNo.IsChecked == true)
                            {
                                sintaxFixer.reorder(true);
                            }
                            else if (ReorderSpecified.IsChecked == true)
                            {
                                byte[] reordering = parseReorderingString(ReorderSpecifiedOrder.Text);            
                                sintaxFixer.reorder(false, reordering);
                            }
                        }
                    }
                    sintaxFixer.Save();
                }
                
                if (EnableHeaderFix.IsChecked == true)
                {
                    HeaderFixer headerfixer = new HeaderFixer(OutputFilename.Text, OutputFilename.Text);
                    headerfixer.HeaderFix(
                        (bool)EnableHeaderSize.IsChecked, 
                        (bool)EnableHeaderComp.IsChecked, 
                        (bool)EnableHeaderChecksum.IsChecked,
                        (bool)EnableHeaderType.IsChecked ? Byte.Parse(RomType.Text, System.Globalization.NumberStyles.HexNumber) : (byte?)null,
                        (bool)EnableHeaderRamsize.IsChecked ? Byte.Parse(RamSize.Text, System.Globalization.NumberStyles.HexNumber) : (byte?)null
                    );
                    headerfixer.Save();
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

        private byte parseFlipStringToXor(string flipString)
        {
            byte xor = 0;
            if (flipString.Substring(0, 2) == "0x") {
                xor = byte.Parse(flipString.Substring(2), System.Globalization.NumberStyles.HexNumber);
            } else {
                foreach (char flipbit in flipString.ToCharArray())
                {
                    xor += (byte)(0x80 >> int.Parse(flipbit.ToString()));
                }
            }
            return xor;
        }

        private byte[] parseFlipStringsToXors(string[] flipStrings)
        {
            byte[] xors = new byte[flipStrings.Length];
            for (int x = 0; x < flipStrings.Length; x++)
            {
                xors[x] = parseFlipStringToXor(flipStrings[x]);
            }
            return xors;
        }

        private void RomType_GotFocus(object sender, RoutedEventArgs e)
        {
            EnableHeaderType.IsChecked = true;
        }

        private void RamSize_GotFocus(object sender, RoutedEventArgs e)
        {
            EnableHeaderRamsize.IsChecked = true;
        }

        private void PopulateErrorMessage(Exception e)
        {
            ErrorMsg.Content = "★ " + e.Message;
        }

        private void BBDTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!bbdMode)
                {
                    OperationsHeader.Content = "BBD mode";
                    bbdMode = true;
                    XorControls.Visibility = Visibility.Hidden;
                    BBDStuff.Visibility = Visibility.Visible;
                }
                else
                {
                    OperationsHeader.Content = "Sintax mode";
                    bbdMode = false;
                    XorControls.Visibility = Visibility.Visible;
                    BBDStuff.Visibility = Visibility.Hidden;
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
