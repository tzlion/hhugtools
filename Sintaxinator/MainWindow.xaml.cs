﻿using System;
using System.Windows;
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

        private byte[] ParseReorderingString(String input)
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
                    BitDescrambler bitDescrambler = new BitDescrambler(InputFilename.Text, OutputFilename.Text);
                    
                    if (EnableFullAuto.IsChecked == true)
                    {
                        byte reorderMode = byte.Parse(BitScrambleMode.Text, System.Globalization.NumberStyles.HexNumber);
                        bitDescrambler.ReorderAllBytes(Reorderings.GetBbdDataReorderings(reorderMode));
                    }
                    
                    if (EnableBBDDescramble.IsChecked == true)
                    {
                        byte[] reordering = ParseReorderingString(BBDBitDescramble.Text);                        
                        bitDescrambler.ReorderAllBytes(reordering);
                    }
                    
                    bitDescrambler.Save();
                    
                    if (EnableFullAuto.IsChecked == true)
                    {
                        SintaxFixer sintaxFixer = new SintaxFixer(OutputFilename.Text, OutputFilename.Text);
                        byte reorderMode = byte.Parse(ReorderMode.Text, System.Globalization.NumberStyles.HexNumber);
                        sintaxFixer.reorder(false, Reorderings.GetBbdBankReorderings(reorderMode));
                        sintaxFixer.Save();
                    }

                    if (EnableReorder.IsChecked == true)
                    {
                        SintaxFixer sintaxFixer = new SintaxFixer(OutputFilename.Text, OutputFilename.Text);
                        if (ReorderAuto.IsChecked == true)
                        {
                            byte reorderMode = byte.Parse(ReorderAutoMode.Text, System.Globalization.NumberStyles.HexNumber);
                            sintaxFixer.reorder(false, Reorderings.GetBbdBankReorderings(reorderMode));
                        }
                        else if (ReorderBankNo.IsChecked == true)
                        {
                            sintaxFixer.reorder(true);
                        }
                        else if (ReorderSpecified.IsChecked == true)
                        {
                            byte[] reordering = ParseReorderingString(ReorderSpecifiedOrder.Text);            
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
                        sintaxFixer.reorder(false, Reorderings.GetSintaxBankReorderings(reorderMode));
                        string[] flipstrings = {  
                            "0x" + ManualBits1.Text, 
                            "0x" + ManualBits2.Text, 
                            "0x" + ManualBits3.Text, 
                            "0x" + ManualBits4.Text
                        };
                        byte[] manualXors = ParseFlipStringsToXors(flipstrings);
                        sintaxFixer.xorAllData(false, manualXors, 64);
                    }
                    else
                    {
                        if (EnableXor.IsChecked == true)
                        {
                            string[] flipstrings = ManualBits.Text.Split(new String[] { "|" }, new StringSplitOptions()); ;
                            byte[] manualXors = ParseFlipStringsToXors(flipstrings);
                            sintaxFixer.xorAllData(false, manualXors, int.Parse(XorRepeat.Text));
                        }
                        if (EnableReorder.IsChecked == true)
                        {
                            if (ReorderAuto.IsChecked == true)
                            {
                                byte reorderMode = byte.Parse(ReorderAutoMode.Text, System.Globalization.NumberStyles.HexNumber);
                                sintaxFixer.reorder(false, Reorderings.GetSintaxBankReorderings(reorderMode));
                            }
                            else if (ReorderBankNo.IsChecked == true)
                            {
                                sintaxFixer.reorder(true);
                            }
                            else if (ReorderSpecified.IsChecked == true)
                            {
                                byte[] reordering = ParseReorderingString(ReorderSpecifiedOrder.Text);            
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

        private byte ParseFlipStringToXor(string flipString)
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

        private byte[] ParseFlipStringsToXors(string[] flipStrings)
        {
            byte[] xors = new byte[flipStrings.Length];
            for (int x = 0; x < flipStrings.Length; x++)
            {
                xors[x] = ParseFlipStringToXor(flipStrings[x]);
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
