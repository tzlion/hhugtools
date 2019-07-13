using System;
using System.Linq;
using Common.Rom;

namespace Sintaxinator.Fixers
{

    class SintaxFixer : RomManipulator
    {
        public SintaxFixer(string inputFilename, string outputFilename) : base(inputFilename, outputFilename) { }

        public void xorAllData(bool auto, byte[] manualXorSet, int repeatCount = 1)
        {
            byte[] processed = { };
            
            int bankCount = this.rom.Length / 0x4000;

            byte[] manualXors = { };
            for (int x = 0; x < repeatCount; x++)
            {
                manualXors = manualXors.Concat(manualXorSet).ToArray();
            }

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

        private byte[] xorData(byte[] origData, byte xor)
        {
            byte[] newData = new byte[origData.Length];
            for(int x=0;x<origData.Length;x++) {
                newData[x] = (byte)(origData[x] ^ xor);
            }
            return newData;
        }
    }
}
