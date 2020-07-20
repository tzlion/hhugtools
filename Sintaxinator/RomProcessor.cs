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
        
        public void ProcessBbdFullAuto(string filename, byte bitScrambleMode, byte reorderMode)
        {
            BitDescrambler bitDescrambler = new BitDescrambler(filename, filename);
            bitDescrambler.ReorderAllBytes(Reorderings.GetBbdDataReorderings(bitScrambleMode));
            bitDescrambler.Save();
            
            BankReorderer reorderer = new BankReorderer(filename, filename);
            reorderer.Reorder(false, Reorderings.GetBbdBankReorderings(reorderMode));
            reorderer.Save();
        }
        
        public void ProcessBbd(string filename, string bitDescramblePattern, byte? reorderMode, bool reorderByBankNo,
            string manualReorderPattern)
        {
            if (bitDescramblePattern != null)
            {
                BitDescrambler bitDescrambler = new BitDescrambler(filename, filename);
                byte[] reordering = ParseReorderingString(bitDescramblePattern);                        
                bitDescrambler.ReorderAllBytes(reordering);
                bitDescrambler.Save();
            }

            if (reorderMode != null || reorderByBankNo || manualReorderPattern != null)
            {
                byte[] reordering = null;
                var checkBankBits = false;
                if (reorderMode != null)
                {
                    reordering = Reorderings.GetBbdBankReorderings(reorderMode);
                }
                else if (reorderByBankNo)
                {
                    checkBankBits = true;
                }
                else if (manualReorderPattern != null)
                {
                    reordering = ParseReorderingString(manualReorderPattern);           
                }
                BankReorderer reorderer = new BankReorderer(filename, filename); 
                reorderer.Reorder(checkBankBits, reordering);
                reorderer.Save();
            }
        }
        
        public void ProcessSintaxFullAuto(string filename, byte reorderMode, byte[] xors)
        {
            BankReorderer bankReorderer = new BankReorderer(filename, filename);
            var usedBanks = bankReorderer.Reorder(false, Reorderings.GetSintaxBankReorderings(reorderMode));
            bankReorderer.Save();
            
            DataXorer dataXorer = new DataXorer(filename, filename);
            dataXorer.XorAllData(false, xors, 64);
            dataXorer.Save();
            
            BankBlanker bankBlanker = new BankBlanker(filename, filename);
            bankBlanker.BlankUnusedBanks(usedBanks);
            bankBlanker.Save();
        }

        public void ProcessSintax(string filename, byte? reorderMode, bool reorderByBankNo, string manualReorderPattern,
            string xorBits, int? xorRepeatCount)
        {
            if (xorBits != null)
            {
                DataXorer dataXorer = new DataXorer(filename, filename);
                string[] flipstrings = xorBits.Split(new [] { "|" }, new StringSplitOptions());
                byte[] manualXors = ParseFlipStringsToXors(flipstrings);
                dataXorer.XorAllData(false, manualXors, xorRepeatCount ?? 0);
                dataXorer.Save();
            }
            
            if (reorderMode != null || reorderByBankNo || manualReorderPattern != null)
            {
                byte[] reordering = null;
                var checkBankBits = false;
                if (reorderMode != null)
                {
                    reordering = Reorderings.GetSintaxBankReorderings(reorderMode);
                }
                else if (reorderByBankNo)
                {
                    checkBankBits = true;
                }
                else if (manualReorderPattern != null)
                {
                    reordering = ParseReorderingString(manualReorderPattern);    
                }        
                BankReorderer bankReorderer = new BankReorderer(filename, filename);
                bankReorderer.Reorder(checkBankBits, reordering);
                bankReorderer.Save();
            }
        }

        public void FixHeader(string filename, bool enableHeaderSize, bool enableHeaderComp,
            bool enableHeaderChecksum, byte? romType, byte? ramSize)
        {
            HeaderFixer headerFixer = new HeaderFixer(filename, filename);
            headerFixer.HeaderFix(
                enableHeaderSize, 
                enableHeaderComp, 
                enableHeaderChecksum,
                romType,
                ramSize
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
                foreach (char flipbit in flipString)
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