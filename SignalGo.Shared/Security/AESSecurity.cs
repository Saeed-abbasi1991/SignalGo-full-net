﻿using System;
using System.IO;
#if (!PORTABLE)
using System.Security.Cryptography;
#endif
using System.Text;

namespace SignalGo.Shared.Security
{
    public static class AESSecurity
    {
        public static byte[] EncryptBytes(byte[] bytes, string key, string IV)
        {
#if (PORTABLE)
            throw new NotSupportedException("not support for this .net standard version!");
#else
            return EncryptBytes(bytes, Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(IV));
#endif
        }

        public static byte[] EncryptBytes(byte[] bytes, byte[] key, byte[] IV)
        {
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
            throw new NotSupportedException("not support for this .net standard version!");
#else
            if (bytes == null || bytes.Length <= 0)
                throw new ArgumentNullException("bytes");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = key;
                rijAlg.IV = IV;
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(bytes, 0, bytes.Length);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
            return encrypted;
#endif
        }

        public static byte[] DecryptBytes(byte[] bytes, string key, string IV)
        {
#if (PORTABLE)
            throw new NotSupportedException("not support for this .net standard version!");
#else
            return DecryptBytes(bytes, Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(IV));
#endif
        }

        public static byte[] DecryptBytes(byte[] bytes, byte[] Key, byte[] IV)
        {
#if (NETSTANDARD || NETCOREAPP || PORTABLE)
            throw new NotSupportedException("not support for this .net standard version!");
#else
            if (bytes == null || bytes.Length <= 0)
                throw new ArgumentNullException("bytes");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            using (RijndaelManaged rijAlg = new RijndaelManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);
                using (MemoryStream memoryResult = new MemoryStream())
                {
                    using (MemoryStream msDecrypt = new MemoryStream(bytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            int read;
                            byte[] buffer = new byte[16 * 1024];
                            while ((read = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                memoryResult.Write(buffer, 0, read);
                            }
                            return memoryResult.ToArray();
                        }
                    }
                }
            }
#endif
        }
    }
}
