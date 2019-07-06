using System;
using System.Linq;
using RomStuff;

namespace SintaxStuff
{
    class OverdumpDetector : RomLoader
    {
        public OverdumpDetector(string inputFilename) : base (inputFilename)
        {
        }

        public string getSizeInfo()
        {
            if ( this.rom.Length == 0 ) {
                return "Zero size dilemma";
            }

            if ((this.rom.Length & (this.rom.Length - 1)) != 0)
            {
                return "ROM size not a power of two, idk how to handle that";
            }

            int size = findSize();
            int sizeDiff = (this.rom.Length / size);
            return
                "File size: " + generateMultiNumberString(this.rom.Length) + "\r\n" +
                "Actual ROM size: " + generateMultiNumberString(size) + "\r\n" +
                "File is " + ( sizeDiff > 1 ? ( (this.rom.Length / size) + "x too big" ) : "juuust right" );
        }

        private string generateMultiNumberString(int number)
        {
            return number + " bytes (" + number.ToString("X") + " hex) / " + (number / 1024) + " kb";
        }


        public int findSize()
        {
            int currentRomSize = this.rom.Length;

            while (currentRomSize > 0)
            {
                string currentHash = "";

                currentRomSize /= 2;

                for (int i = 0; i < (this.rom.Length / currentRomSize); i++)
                {
                    byte[] romChunk = this.rom.Skip(i * currentRomSize).Take(currentRomSize).ToArray();
                    string newHash = UtilityStuff.GetMD5Hash(romChunk);
                    if (currentHash == "") currentHash = newHash;
                    if (newHash != currentHash) return currentRomSize*2; // We find differences at this size so return the next one up
                }
            }

            return 1;
        }
    }
}
