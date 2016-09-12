using System;
using System.Linq;

namespace RomStuff
{
    class DigiSapphTestThing : RomManipulator
    {
        public DigiSapphTestThing(string inputFilename, string outputFilename) : base(inputFilename, outputFilename) { }


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

    }
}
