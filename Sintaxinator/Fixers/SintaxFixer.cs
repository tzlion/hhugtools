using System;
using System.Linq;
using CommonStuff.Rom;

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
                        byte realBankNo = getRealBankNo(curBank);
                        xor = (byte)(bankData[bankData.Length - 1] ^ realBankNo);
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
        public void reorder(bool checkBankBits)
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
                else realBankNo = getRealBankNo(curBank);

                superdata[realBankNo] = bankData;
            }

            byte[] newrom = {};

            foreach(byte[] datapart in superdata) {
                newrom = newrom.Concat(datapart).ToArray();
            }

            this.rom = newrom;

        }

        // I think this only applies for one type
        private byte getRealBankNo(int sequentialBankNo)
        {
            int realBankNo;
            if ( sequentialBankNo < 64 ) {
                realBankNo = sequentialBankNo * 4;
            } else if ( sequentialBankNo < 128 ) {
                realBankNo = ( ( sequentialBankNo - 64 ) * 4 ) + 1;
            } else if ( sequentialBankNo < 196 ) { // inferred for 4mb, untested
                realBankNo = ( ( sequentialBankNo - 128 ) * 4 ) + 2;
            } else { // inferred for 4mb, untested
                realBankNo = ( ( sequentialBankNo - 196 ) * 4 ) + 1;
            }
            return (byte)realBankNo;
        }
   }
}
