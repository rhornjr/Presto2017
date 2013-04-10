using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PrestoCommon.Misc
{
    /// <summary>
    /// AES encryption using RijndaelManaged. Taken from the C# Cookbook (O'Reilly), recipe 17.2.
    /// </summary>
    public static class AesCrypto
    {
        #region [ Private Member Variables ]

        /// <summary>
        /// Default secret key value.
        /// </summary>
        private static readonly byte[] _secretKey = new byte[]
            { 
            191, 204, 8, 44, 63, 128, 2, 214,
            109, 118, 7, 20, 40, 199, 1, 200,
            150, 151, 6, 18, 93, 204, 9, 177,
            112, 188, 4, 49, 51, 167, 3, 134 
            };

        /// <summary>
        /// Default vector value.
        /// </summary>
        private static readonly byte[] _initVector = new byte[]
            { 
            118, 199, 7, 90, 82, 217, 6, 108,
            217, 144, 4, 62, 11, 186, 7, 127
            };

        #endregion

        #region [ Public Methods ]
        /// <summary>
        /// Encrypt a string using <see cref="RijndaelManaged"/>.
        /// </summary>
        /// <param name="stringToEncrypt">The string to encrypt.</param>
        /// <returns>Encrypted version of the original string.</returns>
        public static string Encrypt(string stringToEncrypt)
        {
            byte[] stringToEncryptAsBytes = Encoding.ASCII.GetBytes(stringToEncrypt);
            byte[] originalBytes = null;

            using (MemoryStream memoryStream = new MemoryStream(stringToEncryptAsBytes.Length))
            {
                using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
                {
                    using (ICryptoTransform cryptoTransform = rijndaelManaged.CreateEncryptor(_secretKey, _initVector))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                        {
                            // Write encrypted data to the memory stream
                            cryptoStream.Write(stringToEncryptAsBytes, 0, stringToEncryptAsBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            originalBytes = memoryStream.ToArray();
                        }
                    }
                }
            }

            return Convert.ToBase64String(originalBytes);
        }

        /// <summary>
        /// Decrypt a <see cref="RijndaelManaged"/> string.
        /// </summary>
        /// <param name="encryptedString">The encrypted string.</param>
        /// <returns>The decrypted version of the string.</returns>
        public static string Decrypt(string encryptedString)
        {
            byte[] encryptedStringAsBytes = Convert.FromBase64String(encryptedString);
            byte[] initialText = new byte[encryptedStringAsBytes.Length];

            using (RijndaelManaged rijndaelManaged = new RijndaelManaged())
            {
                using (MemoryStream memoryStream = new MemoryStream(encryptedStringAsBytes))
                {
                    using (ICryptoTransform cryptoTransform = rijndaelManaged.CreateDecryptor(_secretKey, _initVector))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
                        {
                            cryptoStream.Read(initialText, 0, initialText.Length);
                        }
                    }
                }
            }

            string decryptedString = Encoding.ASCII.GetString(initialText, 0, initialText.Length);

            // The decrypted string can contain null terminating characters, so remove them.
            return decryptedString.TrimEnd('\0');
        }

        #endregion
    }
}
