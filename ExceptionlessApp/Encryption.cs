using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionlessApp
{
    /// <summary>
    /// Encrypts strings to SecureStrings
    /// </summary>
    internal static class Encryption
    {
        /// <summary>
        /// Computes SHA-512 hash value
        /// </summary>
        /// <param name="text">text to get hash from</param>
        /// <returns>Returns the hash</returns>
        /// <exception cref="ArgumentNullException">Argument Null Exception</exception>
        public static string GetHash(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentNullException("text");
            }

            SHA512CryptoServiceProvider csp = new SHA512CryptoServiceProvider();
            byte[] data = Encoding.ASCII.GetBytes(text);
            data = csp.ComputeHash(data);
            return Convert.ToBase64String(data);
        }

        /// <summary>
        /// Converts string to SecureString
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>secured string</returns>
        public static SecureString ToSecureString(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            SecureString secure = new SecureString();
            foreach (char c in input)
            {
                secure.AppendChar(c);
            }

            secure.MakeReadOnly();
            return secure;
        }

        /// <summary>
        /// Converts the secure string back to insecure string
        /// </summary>
        /// <param name="input">secure string</param>
        /// <returns>insecure string</returns>
        public static string ToInsecureString(SecureString input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            string returnValue = string.Empty;
            IntPtr ptr = System.Runtime.InteropServices.Marshal.SecureStringToBSTR(input);
            try
            {
                returnValue = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(ptr);
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.ZeroFreeBSTR(ptr);
            }

            return returnValue;
        }
    }
}