using System;
using System.Collections.Generic;

namespace CommonStuff.Rom
{
    class HeaderFixer : RomManipulator
    {
        public HeaderFixer(string inputFilename, string outputFilename) : base(inputFilename, outputFilename) { }

        public void HeaderFix(bool size, bool complement, bool checksum, byte? romType = null, byte? ramSize = null)
        {
            if (size)
            {
                Dictionary<int, byte> romsizes = new Dictionary<int, byte>();

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
                int comp = 0;

                for (int x = 0x0134; x <= 0x014C; x++)
                {
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
 
    }
}
