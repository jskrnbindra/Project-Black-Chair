using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.IO;
using System.Text;


namespace DataCollection
{
    public class QueryStringEncryption
    {
        string EncryptionKey;

        public  QueryStringEncryption()
        {
            EncryptionKey = "RMWI42WJSD042JD";
        }

        public string encryptQueryString(string NakedText)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(NakedText);

            using (Aes aesOb = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                aesOb.Key = pdb.GetBytes(32);
                aesOb.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms,aesOb.CreateEncryptor(),CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    NakedText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return NakedText;
        }

        public string decryptQueryString(string CipherText)
        {

            CipherText = CipherText.Replace(" ", "+");
            byte[] CipherBytes = Convert.FromBase64String(CipherText);

            using (Aes aesOb = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                aesOb.Key = pdb.GetBytes(32);
                aesOb.IV = pdb.GetBytes(16);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aesOb.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(CipherBytes, 0, CipherBytes.Length);
                        cs.Close();
                    }
                    CipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return CipherText;
        }

    }
}