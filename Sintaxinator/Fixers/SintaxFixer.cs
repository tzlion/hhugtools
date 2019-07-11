using System;
using System.Linq;
using Common.Rom;
using Common.Utility;

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

        // This is pretty slow
        public void reorder(bool checkBankBits, byte[] romBankNoReordering = null)
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
                else realBankNo = getRealBankNo(curBank, romBankNoReordering);

                superdata[realBankNo] = bankData;
            }

            byte[] newrom = {};

            foreach(byte[] datapart in superdata) {
                newrom = newrom.Concat(datapart).ToArray();
            }

            this.rom = newrom;

        }

        public byte[] getSintaxBankReorderings(byte? reorderMode)
        {
            // these are from hhugboy
            byte[] reordering00 = {0,7,2,1,4,3,6,5};
            byte[] reordering01 = {7,6,1,0,3,2,5,4};
            byte[] reordering05 = {0,1,6,7,4,5,2,3};
            byte[] reordering07 = {6,7,4,5,2,3,0,1};
            byte[] reordering09 = {3,2,5,4,7,6,1,0};
            byte[] reordering0b = {5,4,7,6,1,0,3,2};
            byte[] reordering0d = {6,7,0,1,2,3,4,5};
            byte[] noReordering = {0,1,2,3,4,5,6,7};
    
            switch(reorderMode & 0x0f) {
                case 0x0D:
                    return reordering0d;
                case 0x09:
                    return reordering09;
                case 0x00:
                    return reordering00;
                case 0x01:
                    return reordering01;
                case 0x05:
                    return reordering05;
                case 0x07:
                    return reordering07;
                case 0x0B:
                    return reordering0b;
                case 0x0F:
                    return noReordering;
                default:
                    throw new Exception("unsupported reordering type");
            }
        }

        private byte getRealBankNo(int sequentialBankNo, byte[] romBankNoReordering)
        {
            return ByteManipulation.ReorderBits((byte)sequentialBankNo, romBankNoReordering);
        }
    }
}
