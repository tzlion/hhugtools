using System;
using System.IO;
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

        private byte[] ParseReorderingString(string input)
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

        private void DoTheThing(object sender, RoutedEventArgs e)
        {
            try
            {
                byte[] rom = File.ReadAllBytes(InputFilename.Text);
                File.WriteAllBytes(OutputFilename.Text, rom);
                
                if (bbdMode)
                {
                    BitDescrambler bitDescrambler = new BitDescrambler(OutputFilename.Text, OutputFilename.Text);
                    
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
                        BankReorderer reorderer = new BankReorderer(OutputFilename.Text, OutputFilename.Text);
                        byte reorderMode = byte.Parse(ReorderMode.Text, System.Globalization.NumberStyles.HexNumber);
                        reorderer.Reorder(false, Reorderings.GetBbdBankReorderings(reorderMode));
                        reorderer.Save();
                    }

                    if (EnableReorder.IsChecked == true)
                    {
                        BankReorderer reorderer = new BankReorderer(OutputFilename.Text, OutputFilename.Text);
                        if (ReorderAuto.IsChecked == true)
                        {
                            byte reorderMode = byte.Parse(ReorderAutoMode.Text, System.Globalization.NumberStyles.HexNumber);
                            reorderer.Reorder(false, Reorderings.GetBbdBankReorderings(reorderMode));
                        }
                        else if (ReorderBankNo.IsChecked == true)
                        {
                            reorderer.Reorder(true);
                        }
                        else if (ReorderSpecified.IsChecked == true)
                        {
                            byte[] reordering = ParseReorderingString(ReorderSpecifiedOrder.Text);            
                            reorderer.Reorder(false, reordering);
                        }
                        reorderer.Save();
                    }
                }
                else
                {
                    if (EnableFullAuto.IsChecked == true)
                    {
                        BankReorderer bankReorderer = new BankReorderer(OutputFilename.Text, OutputFilename.Text);
                        byte reorderMode = byte.Parse(ReorderMode.Text, System.Globalization.NumberStyles.HexNumber);
                        bankReorderer.Reorder(false, Reorderings.GetSintaxBankReorderings(reorderMode));
                        bankReorderer.Save();
                        string[] flipstrings = {  
                            "0x" + ManualBits1.Text, 
                            "0x" + ManualBits2.Text, 
                            "0x" + ManualBits3.Text, 
                            "0x" + ManualBits4.Text
                        };
                        byte[] manualXors = ParseFlipStringsToXors(flipstrings);
                        DataXorer dataXorer = new DataXorer(OutputFilename.Text, OutputFilename.Text);
                        dataXorer.XorAllData(false, manualXors, 64);
                        dataXorer.Save();
                    }
                    else
                    {
                        if (EnableXor.IsChecked == true)
                        {
                            DataXorer dataXorer = new DataXorer(OutputFilename.Text, OutputFilename.Text);
                            string[] flipstrings = ManualBits.Text.Split(new string[] { "|" }, new StringSplitOptions()); ;
                            byte[] manualXors = ParseFlipStringsToXors(flipstrings);
                            dataXorer.XorAllData(false, manualXors, int.Parse(XorRepeat.Text));
                            dataXorer.Save();
                        }
                        if (EnableReorder.IsChecked == true)
                        {
                            BankReorderer bankReorderer = new BankReorderer(OutputFilename.Text, OutputFilename.Text);
                            if (ReorderAuto.IsChecked == true)
                            {
                                byte reorderMode = byte.Parse(ReorderAutoMode.Text, System.Globalization.NumberStyles.HexNumber);
                                bankReorderer.Reorder(false, Reorderings.GetSintaxBankReorderings(reorderMode));
                            }
                            else if (ReorderBankNo.IsChecked == true)
                            {
                                bankReorderer.Reorder(true);
                            }
                            else if (ReorderSpecified.IsChecked == true)
                            {
                                byte[] reordering = ParseReorderingString(ReorderSpecifiedOrder.Text);            
                                bankReorderer.Reorder(false, reordering);
                            }
                            bankReorderer.Save();
                        }
                    }
                }
                
                if (EnableHeaderFix.IsChecked == true)
                {
                    HeaderFixer headerFixer = new HeaderFixer(OutputFilename.Text, OutputFilename.Text);
                    headerFixer.HeaderFix(
                        EnableHeaderSize.IsChecked ?? false, 
                        EnableHeaderComp.IsChecked ?? false, 
                        EnableHeaderChecksum.IsChecked ?? false,
                        (EnableHeaderType.IsChecked ?? false) ? byte.Parse(RomType.Text, System.Globalization.NumberStyles.HexNumber) : (byte?)null,
                        (EnableHeaderRamsize.IsChecked ?? false) ? byte.Parse(RamSize.Text, System.Globalization.NumberStyles.HexNumber) : (byte?)null
                    );
                    headerFixer.Save();
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
