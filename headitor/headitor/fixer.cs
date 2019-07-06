using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;

namespace headitor
{

    class fixer
    {
        private static Dictionary<int, string> romTypes = new Dictionary<int, string>()
            {
                { -1, " " },
                { 0x00, "ROM ONLY" },
                { 0x01, "MBC1" },
                { 0x02, "MBC1+RAM" },
                { 0x03, "MBC1+RAM+BATTERY" },
                { 0x05, "MBC2" },
                { 0x06, "MBC2+BATTERY" },
                { 0x08, "ROM+RAM" },
                { 0x09, "ROM+RAM+BATTERY" },
                { 0x0B, "MMM01" },
                { 0x0C, "MMM01+RAM" },
                { 0x0D, "MMM01+RAM+BATTERY" },
                { 0x0F, "MBC3+TIMER+BATTERY" },
                { 0x10, "MBC3+TIMER+RAM+BATTERY" },
                { 0x11, "MBC3" },
                { 0x12, "MBC3+RAM" },
                { 0x13, "MBC3+RAM+BATTERY" },
                { 0x15, "MBC4" },
                { 0x16, "MBC4+RAM" },
                { 0x17, "MBC4+RAM+BATTERY" },
                { 0x19, "MBC5" },
                { 0x1A, "MBC5+RAM" },
                { 0x1B, "MBC5+RAM+BATTERY" },
                { 0x1C, "MBC5+RUMBLE" },
                { 0x1D, "MBC5+RUMBLE+RAM" },
                { 0x1E, "MBC5+RUMBLE+RAM+BATTERY" },
                { 0xFC, "POCKET CAMERA" },
                { 0xFD, "BANDAI TAMA5" },
                { 0xFE, "HuC3" },
                { 0xFF, "HuC1+RAM+BATTERY" },
            };

        private byte[] rom;

        public fixer( string inputFilename )
        {
            if (!File.Exists(inputFilename))
            {
                throw new Exception("input file does not exist");
            }
            // read the file here
            this.rom = File.ReadAllBytes(inputFilename);
        }

        public static Dictionary<int, string> getRomTypes()
        {
            return romTypes;
        }

        public int getCurrentRomType()
        {
            return this.rom[0x0147];
        }

        public int getCurrentRomSize()
        {
            return this.rom[0x0148];
        }

        public int getCurrentRamSize()
        {
            return this.rom[0x0149];
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

        public void save(string outputFilename, bool openAfterwards = false)
        {
            if (!Directory.Exists(new FileInfo(outputFilename).DirectoryName))
            {
                throw new Exception("output path does not exist");
            }

            File.WriteAllBytes(outputFilename,this.rom);
            if (openAfterwards)
            {
                System.Diagnostics.Process.Start(outputFilename);
            }

        }
    }
}
