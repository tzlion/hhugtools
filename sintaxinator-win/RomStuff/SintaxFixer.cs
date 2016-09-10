using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace RomStuff
{

    class SintaxFixer : RomManipulator
    {
        public SintaxFixer(string inputFilename, string outputFilename) : base(inputFilename, outputFilename) { }

        public void flipBits(bool auto, string manualstring ="", int repeatCount = 1)
        {

            string origManualString = manualstring;
            manualstring = "";
            for (int x = 1; x <= repeatCount; x++)
            {
                manualstring += origManualString;
                if (x != repeatCount) manualstring += "|";
            }

            byte[] processed = { };

            bool[][] manualflips = {};

            if (!auto)
            {
                string[] flipstrings = manualstring.Split(new String[]{"|"}, new StringSplitOptions());
                manualflips = new bool[flipstrings.Length][];
                for (int x = 0; x < flipstrings.Length; x++)
                {
                    bool[] flip = new bool[8];
                    foreach(char flipbit in flipstrings[x].ToCharArray()) 
                    {
                        flip[int.Parse(flipbit.ToString())] = true;
                    }
                    manualflips[x] = flip;
                }
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
                    bool[] flips;
                    if (auto)
                    {
                        byte realBankNo = getRealBankNo(curBank);
                        flips = determineBitsToFlip(bankData[bankData.Length-1],realBankNo);
                        // Auto mode
                    }
                    else
                    {
                        int flipno = (int)(((float)curBank / (float)bankCount) * (float)manualflips.Count());
                        flips = manualflips[flipno];
                        // Manual mode
                    }
                    processed = processed.Concat(bitinvert(bankData,flips)).ToArray();
                }
            }

            this.rom = processed;

        }

        private bool[] determineBitsToFlip(byte byteItIs, byte byteItShouldBe)
        {
            // okay binary operations make my head hurt so im just acting like this is a string here

            string binByte1 = Convert.ToString(byteItIs, 2).PadLeft(8, '0');
            string binByte2 = Convert.ToString(byteItShouldBe, 2).PadLeft(8, '0'); 

            bool[] bitsToFlip=new bool[8];
            for(int x=0;x<=7;x++) {
                if (binByte1[x] != binByte2[x])
                {
                    bitsToFlip[x] = true;
                }
                else
                {
                    bitsToFlip[x] = false;
                }
            }

            return bitsToFlip;
        }

        private byte[] bitswap(byte[] origData)
        {

            byte[] newData = new byte[origData.Length];

            for (int x = 0; x < origData.Length; x++)
            {
                byte thisbyte = origData[x];

                char[] binbyte = Convert.ToString(thisbyte, 2).PadLeft(8, '0').ToCharArray(); ;

                char oldbit2 = binbyte[2];

                binbyte[2] = binbyte[6];
                binbyte[6] = oldbit2;

                newData[x] = Convert.ToByte(new string(binbyte), 2);
            }

            return newData;

        }

        public void testswap() // for Digi Sapphire // Can't remember what the thought behind this even was but okay
        {

            byte[] processed = { };

            int bankCount = this.rom.Length / 0x4000;
            int bankto8 = 0;
            for (int curBank = 0; curBank < bankCount; curBank++)
            {
                
                byte[] bankData = this.rom.Skip(0x4000 * curBank).Take(0x4000).ToArray();
                if (bankto8 == 0)
                {
                    processed = processed.Concat(bankData).ToArray();
                }
                else if (bankto8 == 4)
                {
                    processed = processed.Concat(bitswap(bankData)).ToArray();
                }
                else
                {
                    processed = processed.Concat(bankData).ToArray();
                }
                bankto8++; if (bankto8 >= 8) bankto8 = 0;
            }

            this.rom = processed;
        }


        private byte[] bitinvert(byte[] origData, bool[] flips)
        {

            byte[] newData = new byte[origData.Length];

            for(int x=0;x<origData.Length;x++) {
                byte thisbyte = origData[x];

                char[] binbyte = Convert.ToString(thisbyte, 2).PadLeft(8, '0').ToCharArray(); ;

                for(int y=0;y<8;y++) {
                    if(flips[y]) {
                        if(binbyte[y] == '0') {
                            binbyte[y] = '1';
                        } else {
                            binbyte[y] = '0';
                        }
                    } 
                }

                newData[x] = Convert.ToByte(new string(binbyte), 2);
            }

            return newData;

        }


        // This is way slower than bitflipping. investigate why?
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
