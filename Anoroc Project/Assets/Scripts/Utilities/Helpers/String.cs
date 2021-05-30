/*
    MIT License

    Copyright (c) 2021 Marx Jason

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.

*/

using System.Linq;
using System.Text.RegularExpressions;


namespace Utilities
{
    public static class String
    {
        private static readonly System.Random SysRandom = new System.Random();

        private static readonly Regex sWhitespace = new Regex(@"\s+");
        private static readonly Regex sAlphanumeric = new Regex(@"[^a-zA-Z0-9.-]");
        private static readonly Regex sRepeatedDots = new Regex(@"\.+");

        /// <summary>
        /// Removes any white string from the given input
        /// </summary>
        /// <param name="input">The string to remove whitespaces from</param>
        /// <returns>The processed string</returns>
        public static string RemoveWhitespace(string input)
        {
            return sWhitespace.Replace(input, "");
        }

        /// <summary>
        /// Removes any character that is not AlphaNumeric (A-Z | 0-9) from given input.
        /// </summary>
        /// <param name="input">The string to remove characters from</param>
        /// <returns>The processed string</returns>
        public static string RemoveNonAlphaNumeric(string input)
        {
            return sRepeatedDots.Replace(sAlphanumeric.Replace(sWhitespace.Replace(input, "."), ""), ".");
        }


        /// <summary>
        /// Generates a random AlphaNumeric String.
        /// </summary>
        /// <param name="length">The length of the generated string</param>
        /// <returns>The random string</returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[SysRandom.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Checks wether the given input string is empty or not.
        /// </summary>
        /// <param name="str">The input to check.</param>
        /// <returns>True, if the string is empty; False otherwhise.</returns>
        public static bool IsEmpty(this string str)
        {
            return str == null || RemoveWhitespace(str) == "";
        }


        /// <summary>
        /// Repeat given string n-times
        /// </summary>
        /// <param name="str">The string to repaet</param>
        /// <param name="amount">The amount to repeat the string for</param>
        /// <returns>The final string</returns>
        public static string Repeat(this string str, int amount)
        {
            return string.Concat(Enumerable.Repeat(str, amount));
        }
    }
}
