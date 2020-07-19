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
        
        public void ProcessBbd(string filename, bool enableFullAuto, byte bitScrambleMode, bool enableBbdDescramble,
            string bbdBitDescramble, byte reorderMode, bool enableReorder, bool reorderAuto, byte reorderAutoMode,
            bool reorderBankNo, bool reorderSpecified, string reorderSpecifiedOrder)
        {
            BitDescrambler bitDescrambler = new BitDescrambler(filename, filename);
            
            if (enableFullAuto)
            {
                bitDescrambler.ReorderAllBytes(Reorderings.GetBbdDataReorderings(bitScrambleMode));
            }
            
            if (enableBbdDescramble)
            {
                byte[] reordering = ParseReorderingString(bbdBitDescramble);                        
                bitDescrambler.ReorderAllBytes(reordering);
            }
            
            bitDescrambler.Save();
            
            if (enableFullAuto)
            {
                BankReorderer reorderer = new BankReorderer(filename, filename);
                reorderer.Reorder(false, Reorderings.GetBbdBankReorderings(reorderMode));
                reorderer.Save();
            }

            if (enableReorder)
            {
                BankReorderer reorderer = new BankReorderer(filename, filename);
                if (reorderAuto)
                {
                    reorderer.Reorder(false, Reorderings.GetBbdBankReorderings(reorderAutoMode));
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
        
        public void ProcessSintax(string filename, bool enableFullAuto, byte reorderMode, bool enableReorder,
            bool reorderAuto, byte reorderAutoMode, bool reorderBankNo, bool reorderSpecified,
            string reorderSpecifiedOrder, string[] manualBitsArray, bool enableXor, string manualBits,
            string xorRepeatText)
        {
            if (enableFullAuto)
            {
                BankReorderer bankReorderer = new BankReorderer(filename, filename);
                bankReorderer.Reorder(false, Reorderings.GetSintaxBankReorderings(reorderMode));
                bankReorderer.Save();
                string[] flipstrings = {  
                    "0x" + manualBitsArray[0], 
                    "0x" + manualBitsArray[1], 
                    "0x" + manualBitsArray[2], 
                    "0x" + manualBitsArray[3], 
                };
                byte[] manualXors = ParseFlipStringsToXors(flipstrings);
                DataXorer dataXorer = new DataXorer(filename, filename);
                dataXorer.XorAllData(false, manualXors, 64);
                dataXorer.Save();
            }
            else
            {
                if (enableXor)
                {
                    DataXorer dataXorer = new DataXorer(filename, filename);
                    string[] flipstrings = manualBits.Split(new string[] { "|" }, new StringSplitOptions()); ;
                    byte[] manualXors = ParseFlipStringsToXors(flipstrings);
                    dataXorer.XorAllData(false, manualXors, int.Parse(xorRepeatText));
                    dataXorer.Save();
                }
                if (enableReorder)
                {
                    BankReorderer bankReorderer = new BankReorderer(filename, filename);
                    if (reorderAuto)
                    {
                        bankReorderer.Reorder(false, Reorderings.GetSintaxBankReorderings(reorderAutoMode));
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

        public void FixHeader(string filename, bool enableHeaderSize, bool enableHeaderComp,
            bool enableHeaderChecksum, bool enableHeaderType, bool enableHeaderRamsize, byte romTypeText,
            byte ramSizeText)
        {
            HeaderFixer headerFixer = new HeaderFixer(filename, filename);
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