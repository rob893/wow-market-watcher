using System;
using System.Text;

namespace WoWMarketWatcher.API.Extensions
{
    /// <summary>
    /// Extensions for primitive types.
    /// </summary>
    public static class PrimitiveExtensions
    {
        /// <summary>
        /// Converts a base64 string representing an int to an int.
        /// </summary>
        /// <param name="str">A valid base64 encoded int.</param>
        /// <returns>The int value of the base64 encoded int.</returns>
        public static int ConvertToInt32FromBase64(this string str)
        {
            try
            {
                return BitConverter.ToInt32(Convert.FromBase64String(str), 0);
            }
            catch
            {
                throw new ArgumentException($"{str} is not a valid base 64 encoded int32.");
            }
        }

        /// <summary>
        /// Converts a base64 string representing an string to a string.
        /// </summary>
        /// <param name="str">A valid base64 encoded string.</param>
        /// <returns>The string value of the base64 encoded string.</returns>
        public static string ConvertToStringFromBase64(this string str)
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(str));
            }
            catch
            {
                throw new ArgumentException($"{str} is not a valid base 64 encoded string.");
            }
        }

        /// <summary>
        /// Validates if a string is a valid base64 encoded int32.
        /// </summary>
        /// <param name="str">A string to test.</param>
        /// <returns>If the string is a valid base64 encoded int or not.</returns>
        public static bool IsValidBase64EncodedInt32(this string str)
        {
            try
            {
                _ = str.ConvertToInt32FromBase64();
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// Validates if a string is a valid base64 encoded string.
        /// </summary>
        /// <param name="str">A string to test.</param>
        /// <returns>If the string is a valid base64 encoded string or not.</returns>
        public static bool IsValidBase64EncodedString(this string str)
        {
            try
            {
                _ = str.ConvertToStringFromBase64();
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        /// <summary>
        /// Converts an int to a base64 string.
        /// </summary>
        /// <param name="i">The int to convert.</param>
        /// <returns>A base64 encoded string of the int.</returns>
        public static string ConvertToBase64(this int i)
        {
            return Convert.ToBase64String(BitConverter.GetBytes(i));
        }

        /// <summary>
        /// Base64 encodes a string.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>A base64 encoded string of the string.</returns>
        public static string ConvertToBase64(this string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }
    }
}