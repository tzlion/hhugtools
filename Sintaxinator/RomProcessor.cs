using System;
using System.IO;
using Common.Rom;
using Sintaxinator.Fixers;

namespace Sintaxinator
{
    public class RomProcessor
    {
        public void CopyRom(string inputFilename, string outputFilename)
        {
            byte[] rom = File.ReadAllBytes(inputFilename);
            File.WriteAllBytes(outputFilename, rom);
        }
        
        public void ProcessBbd(string outputFilename, bool enableFullAuto, byte bitScrambleMode,
            bool enableBbdDescramble, string bbdBitDescramble, byte reorderModeText, bool enableReorder,
            bool reorderAuto, byte reorderAutoModeText, bool reorderBankNo, bool reorderSpecified,
            string reorderSpecifiedOrder)
        {
            BitDescrambler bitDescrambler = new BitDescrambler(outputFilename, outputFilename);
            
            if (enableFullAuto)
            {
                byte reorderMode = bitScrambleMode;
                bitDescrambler.ReorderAllBytes(Reorderings.GetBbdDataReorderings(reorderMode));
            }
            
            if (enableBbdDescramble)
            {
                byte[] reordering = ParseReorderingString(bbdBitDescramble);                        
                bitDescrambler.ReorderAllBytes(reordering);
            }
            
            bitDescrambler.Save();
            
            if (enableFullAuto)
            {
                BankReorderer reorderer = new BankReorderer(outputFilename, outputFilename);
                byte reorderMode = reorderModeText;
                reorderer.Reorder(false, Reorderings.GetBbdBankReorderings(reorderMode));
                reorderer.Save();
            }

            if (enableReorder)
            {
                BankReorderer reorderer = new BankReorderer(outputFilename, outputFilename);
                if (reorderAuto)
                {
                    byte reorderMode = reorderAutoModeText;
                    reorderer.Reorder(false, Reorderings.GetBbdBankReorderings(reorderMode));
                }
                else if (reorderBankNo)
                {
                    reorderer.Reorder(true);
                }
                else if (reorderSpecified)
                {
                    byte[] reordering = ParseReorderingString(reorderSpecifiedOrder);            
                    reorderer.Reorder(false, reordering);
                }
                reorderer.Save();
            }
        }
        
        public void ProcessSintax(string outputFilename, bool enableFullAuto, byte reorderModeText,
            bool enableReorder, bool reorderAuto, byte reorderAutoModeText, bool reorderBankNo, bool reorderSpecified,
            string reorderSpecifiedOrder, string manualBits1, string manualBits2, string manualBits3,
            string manualBits4, bool enableXor, string manualBits, string xorRepeatText)
        {
            if (enableFullAuto)
            {
                BankReorderer bankReorderer = new BankReorderer(outputFilename, outputFilename);
                byte reorderMode = reorderModeText;
                bankReorderer.Reorder(false, Reorderings.GetSintaxBankReorderings(reorderMode));
                bankReorderer.Save();
                string[] flipstrings = {  
                    "0x" + manualBits1, 
                    "0x" + manualBits2, 
                    "0x" + manualBits3, 
                    "0x" + manualBits4
                };
                byte[] manualXors = ParseFlipStringsToXors(flipstrings);
                DataXorer dataXorer = new DataXorer(outputFilename, outputFilename);
                dataXorer.XorAllData(false, manualXors, 64);
                dataXorer.Save();
            }
            else
            {
                if (enableXor)
                {
                    DataXorer dataXorer = new DataXorer(outputFilename, outputFilename);
                    string[] flipstrings = manualBits.Split(new string[] { "|" }, new StringSplitOptions()); ;
                    byte[] manualXors = ParseFlipStringsToXors(flipstrings);
                    dataXorer.XorAllData(false, manualXors, int.Parse(xorRepeatText));
                    dataXorer.Save();
                }
                if (enableReorder)
                {
                    BankReorderer bankReorderer = new BankReorderer(outputFilename, outputFilename);
                    if (reorderAuto)
                    {
                        byte reorderMode = reorderAutoModeText;
                        bankReorderer.Reorder(false, Reorderings.GetSintaxBankReorderings(reorderMode));
                    }
                    else if (reorderBankNo)
                    {
                        bankReorderer.Reorder(true);
                    }
                    else if (reorderSpecified)
                    {
                        byte[] reordering = ParseReorderingString(reorderSpecifiedOrder);            
                        bankReorderer.Reorder(false, reordering);
                    }
                    bankReorderer.Save();
                }
            }
        }

        public void FixHeader(string outputFilename, bool enableHeaderSize, bool enableHeaderComp,
            bool enableHeaderChecksum, bool enableHeaderType, bool enableHeaderRamsize, byte romTypeText,
            byte ramSizeText)
        {
            HeaderFixer headerFixer = new HeaderFixer(outputFilename, outputFilename);
            headerFixer.HeaderFix(
                enableHeaderSize, 
                enableHeaderComp, 
                enableHeaderChecksum,
                enableHeaderType ? romTypeText : (byte?)null,
                enableHeaderRamsize ? ramSizeText : (byte?)null
            );
            headerFixer.Save();
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
    }
}