using System.Linq;
using RomStuff;

namespace SintaxStuff
{
    class BBDFixer : RomManipulator
    {
        public BBDFixer(string inputFilename, string outputFilename) : base(inputFilename, outputFilename) { }

        public void testFix()
        {

            byte[] processed = { };

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
                    processed = processed.Concat(manipData(bankData)).ToArray();
                }
            }

            this.rom = processed;
        }

        private byte[] manipData(byte[] origData)
        {
            byte[] newData = new byte[origData.Length];
            for (int x = 0; x < origData.Length; x++)
            {
               // newData[x] = (byte)((origData[x] & 0xDB) + ((origData[x] & 0x20) >> 3) + ((origData[x] & 0x04) << 3)); // This SHOULD swap bits 2 and 5 around (numbered 01234567) // for DIGIMON
               // newData[x] = (byte)((origData[x] & 0xED) + ((origData[x] & 0x10) >> 3) + ((origData[x] & 0x02) << 3)); // This SHOULD swap bits 3 and 6 around (numbered 01234567) // for HARRY
              //  newData[x] = switchOrder(origData[x], new byte[] { 0,1,5,3,4,6,2,7 }); // for GAROU // Garou is not fine

                newData[x] = switchOrder(origData[x], new byte[] { 0,1,5,3,4,2,6,7 }); // Simplified test for Digimon // Yeah that works, coolz

            }
            return newData;
        }

        // VON HHUGBOY
        byte switchOrder(byte input, byte[] reorder)
        {
            byte newbyte = 0;
            for (byte x = 0; x < 8; x++)
            {
                newbyte += (byte)( (byte)((input >> (7 - reorder[x])) & 1) << (byte)(7 - x) );
            }

            return newbyte;
        }
    }
}
