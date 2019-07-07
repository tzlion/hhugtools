using System;
using System.Linq;
using Common.Utility;
using Common.Rom;

namespace Common.Rom
{
    class OverdumpDetector : RomLoader
    {
        public OverdumpDetector(string inputFilename) : base (inputFilename)
        {
        }

        public string GetSizeInfo()
        {
            if ( this.rom.Length == 0 ) {
                return "Zero size file";
            }

            if ((this.rom.Length & (this.rom.Length - 1)) != 0)
            {
                return "File size not a power of two, can't currently handle that";
            }

            int size = FindSize();
            int sizeDiff = (this.rom.Length / size);
            return
                "File size: " + generateMultiNumberString(this.rom.Length) + "\r\n" +
                "Unique data size: " + generateMultiNumberString(size) + "\r\n" +
                "File is " + ( sizeDiff > 1 ? ( (this.rom.Length / size) + "x too big" ) : "OK" );
        }

        private string generateMultiNumberString(int number)
        {
            return number + " bytes (" + number.ToString("X") + " hex) / " + (number / 1024) + " kb";
        }


        private int FindSize()
        {
            int currentRomSize = this.rom.Length;

            while (currentRomSize > 0)
            {
                string currentHash = "";

                currentRomSize /= 2;

                for (int i = 0; i < (this.rom.Length / currentRomSize); i++)
                {
                    byte[] romChunk = this.rom.Skip(i * currentRomSize).Take(currentRomSize).ToArray();
                    string newHash = Hashing.GetMd5Hash(romChunk);
                    if (currentHash == "") currentHash = newHash;
                    if (newHash != currentHash) return currentRomSize*2; // We find differences at this size so return the next one up
                }
            }

            return 1;
        }
    }
}
