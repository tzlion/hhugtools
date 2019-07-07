using System;
using System.Linq;
using Common.Rom;
using Common.Utility;

namespace Sintaxinator.Fixers
{

    class SintaxFixer : RomManipulator
    {
        public SintaxFixer(string inputFilename, string outputFilename) : base(inputFilename, outputFilename) { }

        public void flipBits(bool auto, string manualstring ="", int repeatCount = 1)
        {

            byte[] processed = { };

            byte[] manualXors = {};

            if (!auto)
            {
                string[] flipstrings = buildFlipStringArray(manualstring,repeatCount);
                manualXors = parseFlipStringsToXors(flipstrings);
            }

            int bankCount = this.rom.Length / 0x4000;

            for (int curBank = 0; curBank < bankCount; curBank++)
            {
                byte[] bankData = this.rom.Skip(0x4000 * curBank).Take(0x4000).ToArray();
                if (curBank == 0) // Header
                {
                    processed = processed.Concat(bankData).ToArray();
                }
                else
                {
                    byte xor;
                    if (auto)
                    {
                        throw new Exception("not working atm");
                        //byte realBankNo = getRealBankNo(curBank);
                        //xor = (byte)(bankData[bankData.Length - 1] ^ realBankNo);
                        // Auto mode
                    }
                    else
                    {
                        int xorNo = (int)(((float)curBank / (float)bankCount) * (float)manualXors.Count());
                        xor = manualXors[xorNo];
                        // Manual mode
                    }
                    processed = processed.Concat(xorData(bankData,xor)).ToArray();
                }
            }

            this.rom = processed;

        }

        private string[] buildFlipStringArray(string inputString, int repeatCount)
        {
            string outputString = "";
            for (int x = 1; x <= repeatCount; x++)
            {
                outputString += inputString;
                if (x != repeatCount) outputString += "|";
            }
            return outputString.Split(new String[] { "|" }, new StringSplitOptions()); ;
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

        private byte[] xorData(byte[] origData, byte xor)
        {
            byte[] newData = new byte[origData.Length];
            for(int x=0;x<origData.Length;x++) {
                newData[x] = (byte)(origData[x] ^ xor);
            }
            return newData;
        }

        // This is pretty slow
        public void reorder(bool checkBankBits, byte? reorderMode)
        {

            byte[] blankrompart = new byte[0x4000];

            byte[][] superdata = new byte[256][];

            for(int x=1;x<=255;x++) {
                superdata[x] = blankrompart;
            }

            int bankCount = this.rom.Length / 0x4000;

            for (int curBank = 0; curBank < bankCount; curBank++)
            {
                byte[] bankData = this.rom.Skip(0x4000 * curBank).Take(0x4000).ToArray();

                byte realBankNo;

                if (curBank == 0) realBankNo = 0; // Header
                else if (checkBankBits) realBankNo = bankData[bankData.Length-1];
                else realBankNo = getRealBankNo(curBank, reorderMode);

                superdata[realBankNo] = bankData;
            }

            byte[] newrom = {};

            foreach(byte[] datapart in superdata) {
                newrom = newrom.Concat(datapart).ToArray();
            }

            this.rom = newrom;

        }

        private byte getRealBankNo(int sequentialBankNo, byte? reorderMode)
        {
            // these are from hhugboy
            byte[] reordering00 = {0,7,2,1,4,3,6,5};
            byte[] reordering01 = {7,6,1,0,3,2,5,4};
            byte[] reordering05 = {0,1,6,7,4,5,2,3}; // Not 100% on this one
            byte[] reordering07 = {5,7,4,6,2,3,0,1}; // 5 and 7 unconfirmed
            byte[] reordering09 = {3,2,5,4,7,6,1,0};
            byte[] reordering0b = {5,4,7,6,1,0,3,2}; // 5 and 6 unconfirmed
            byte[] reordering0d = {6,7,0,1,2,3,4,5};
            byte[] noReordering = {0,1,2,3,4,5,6,7};
            
            byte[] romBankNoReordering;

            switch(reorderMode & 0x0f) {
                case 0x0D:
                    romBankNoReordering = reordering0d;
                    break;
                case 0x09:
                    romBankNoReordering = reordering09;
                    break;
                case 0x00:
                    romBankNoReordering = reordering00;
                    break;
                case 0x01:
                    romBankNoReordering = reordering01;
                    break;
                case 0x05:
                    romBankNoReordering = reordering05;
                    break;
                case 0x07:
                    romBankNoReordering = reordering07;
                    break;
                case 0x0B:
                    romBankNoReordering = reordering0b;
                    break;
                case 0x0F:
                default:
                    romBankNoReordering = noReordering;
                    break;
            }

            return ByteManipulation.ReorderBits((byte)sequentialBankNo, romBankNoReordering);
        }
    }
}
