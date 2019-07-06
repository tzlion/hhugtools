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

    }
}
