using System;
using System.Security.Cryptography;

namespace CommonStuff.Utility
{
    class Hashing
    {
        public static string GetMd5Hash(byte[] data)
        {
            byte[] computedHash = new MD5CryptoServiceProvider().ComputeHash(data);
            return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        }
    }
}
