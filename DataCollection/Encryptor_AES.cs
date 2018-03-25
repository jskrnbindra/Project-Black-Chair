using System;
using System.Security.Cryptography;
using System.IO;

namespace DataCollection
{
    public class Encryptor_AES
    {
        public byte[] EncryptToBytes(string NakedText, byte[] key, byte[] IV)
        {
            if (NakedText == null || NakedText.Length <= 0)
                throw new ArgumentNullException("NakedText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            byte[] Encrypted;

            using (Aes AESob = Aes.Create())
            {
                AESob.Key = key;
                AESob.IV = IV;

                ICryptoTransform encryptor = AESob.CreateEncryptor(AESob.Key, AESob.IV);

                using (MemoryStream MemStream = new MemoryStream())
                {
                    using (CryptoStream CrypStream = new CryptoStream(MemStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(CrypStream))
                        {
                            sw.Write(NakedText);
                        }
                        Encrypted = MemStream.ToArray();
                    }
                }
            }
            return Encrypted;
        }

        public string DecryptToString(byte[] CipherText, byte[] key, byte[] IV)
        {
            if (CipherText == null || CipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string DecryptedText = null;

            using (Aes Aesob = Aes.Create())
            {
                Aesob.Key = key;
                Aesob.IV = IV;

                ICryptoTransform decryptor = Aesob.CreateDecryptor(Aesob.Key, Aesob.IV);

                using (MemoryStream MemStream = new MemoryStream(CipherText))
                {
                    using (CryptoStream CrypStream = new CryptoStream(MemStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(CrypStream))
                        {
                            DecryptedText = sr.ReadToEnd();
                        }
                    }
                }
            }
            return DecryptedText;
        }
    }
}