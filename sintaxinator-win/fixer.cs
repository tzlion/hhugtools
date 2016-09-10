using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace sintaxinator_win
{

    class fixer
    {
        private string outputFilename;
        private byte[] rom;

        public fixer(string inputFilename, string outputFilename)
        {
            if (!File.Exists(inputFilename))
            {
                throw new Exception("input file does not exist");
            }
            if (!Directory.Exists(new FileInfo(outputFilename).DirectoryName))
            {
                throw new Exception("output path does not exist");
            }
            this.outputFilename = outputFilename;
            // Also read the file here
            this.rom = File.ReadAllBytes(inputFilename);
        }

        public void bankChecksums()
        {
            // This doesnt really have shit to do with anything else here

            int bankCount = this.rom.Length / 0x4000;

            StreamWriter txt = File.CreateText(this.outputFilename + ".txt");

            for (int curBank = 0; curBank < bankCount; curBank++)
            {
                byte[] bankData = this.rom.Skip(0x4000 * curBank).Take(0x4000).ToArray();

                txt.WriteLine(curBank.ToString("X2") + " " + GetMD5Hash(bankData));
            }

            txt.Close();

            System.Diagnostics.Process.Start(outputFilename + ".txt");


        }

        // Nicked
        private static string GetMD5Hash(byte[] data)
        {

            byte[] computedHash = new MD5CryptoServiceProvider().ComputeHash(data);
            var sBuilder = new StringBuilder();
            foreach (byte b in computedHash)
            {
                sBuilder.Append(b.ToString("x2").ToLower());
            }
            return sBuilder.ToString();
        }

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

        public void headerFix(bool size, bool complement, bool checksum, byte? romType = null, byte? ramSize = null)
        {
            if (size)
            {
                Dictionary<int,byte> romsizes = new Dictionary<int,byte>();

                romsizes[0x1000000] = 0x09; // 16mb
                romsizes[0x800000] = 0x08; // 8mb
                romsizes[0x400000] = 0x07; // 4mb
                romsizes[0x200000] = 0x06; // 2mb
                romsizes[0x100000] = 0x05; // 1mb
                romsizes[0x80000] = 0x04; // 512kb
                romsizes[0x40000] = 0x03; // 256kb
                romsizes[0x20000] = 0x02; // 128kb
                romsizes[0x10000] = 0x01; // 64kb
                romsizes[0x8000] = 0x00; // 32kb

                if (romsizes.ContainsKey(this.rom.Length))
                {
                    this.rom[0x0148] = romsizes[this.rom.Length];
                }
                else
                {
                    // error?
                }
            }

            if (ramSize != null)
            {
                this.rom[0x149] = (byte)ramSize;
            }

            if (romType != null)
            {
                this.rom[0x147] = (byte)romType;
            }


            if (complement)
            {
                int comp=0;

                for(int x = 0x0134; x <= 0x014C; x++) {
                    comp = comp - this.rom[x] - 1;
                }

                //  x=0:FOR i=0134h TO 014Ch:x=x-MEM[i]-1:NEXT

                // fix the complement
                byte[] compbytes = BitConverter.GetBytes(comp);
                this.rom[0x14D] = compbytes[0];
            }

            if (checksum)
            {
                int cs = 0;
                for (int x = 0; x < this.rom.Length; x++)
                {
                    if (x != 0x014E && x != 0x014F)
                    {
                        cs += this.rom[x];
                    }
                }

                byte[] csbytes = BitConverter.GetBytes(cs);
                if (csbytes.Length < 2)
                {
                    csbytes[1] = 0;
                }
                this.rom[0x14E] = csbytes[1];
                this.rom[0x14F] = csbytes[0];
                
                // Checksum (higher byte first) produced by adding all bytes of   
                // a cartridge except for two checksum bytes together and taking
                // two lower bytes of the result.
            }

        }

        public void save(bool openAfterwards = false)
        {
            File.WriteAllBytes(this.outputFilename,this.rom);
            if (openAfterwards)
            {
                System.Diagnostics.Process.Start(this.outputFilename);
            }

        }
    }
}
