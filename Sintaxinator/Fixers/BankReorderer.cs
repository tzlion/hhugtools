using System.Linq;
using Common.Rom;
using Common.Utility;

namespace Sintaxinator.Fixers
{
    class BankReorderer : RomManipulator
    {
        public BankReorderer(string inputFilename, string outputFilename) : base(inputFilename, outputFilename) { }

        // This is pretty slow
        public bool[] Reorder(bool checkBankBits, byte[] romBankNoReordering = null, bool tempUseOldReordering = false)
        {

            bool[] banksUsed = new bool[256];

            byte[] blankrompart = new byte[0x4000];

            byte[][] superdata = new byte[256][];

            for(int x=1;x<=255;x++) {
                superdata[x] = blankrompart;
                banksUsed[x] = false;
            }

            int bankCount = this.rom.Length / 0x4000;

            for (int curBank = 0; curBank < bankCount; curBank++)
            {
                byte[] bankData = this.rom.Skip(0x4000 * curBank).Take(0x4000).ToArray();

                byte realBankNo;

                if (curBank == 0) realBankNo = 0; // Header
                else if (checkBankBits) realBankNo = bankData[bankData.Length-1];
                else if (tempUseOldReordering) realBankNo = getRealBankNoOld(curBank);
                else realBankNo = getRealBankNo(curBank, romBankNoReordering);
                
                superdata[realBankNo] = bankData;
                banksUsed[realBankNo] = true;
            }

            byte[] newrom = {};

            foreach(byte[] datapart in superdata) {
                newrom = newrom.Concat(datapart).ToArray();
            }

            this.rom = newrom;

            return banksUsed;

        }

        private byte getRealBankNo(int sequentialBankNo, byte[] romBankNoReordering)
        {
            return ByteManipulation.ReorderBits((byte)sequentialBankNo, romBankNoReordering);
        }
        
        // this still works for harry potter and its "proper" reordering of 1d does not...
        private byte getRealBankNoOld(int sequentialBankNo)
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