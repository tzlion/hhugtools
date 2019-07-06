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
    }
}
