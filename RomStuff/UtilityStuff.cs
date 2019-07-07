using System;
using System.Security.Cryptography;

namespace RomStuff
{
    class UtilityStuff
    {
        public static string GetMD5Hash(byte[] data)
        {
            byte[] computedHash = new MD5CryptoServiceProvider().ComputeHash(data);
            return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        }
        
        public static byte reorderBits(byte input, byte[] reorder)
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
