/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using System.Text;

namespace TouchScript.Utils
{
    /// <summary>
    /// Utility methods to deal with binary data.
    /// </summary>
    public static class BinaryUtils
    {
        /// <summary>
        /// Formats an integer value to a binary string.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <param name="builder">The string builder to use.</param>
        /// <param name="digits">The number of digits to include in the string.</param>
        public static void ToBinaryString(uint value, StringBuilder builder, int digits = 32)
        {
            int i = digits - 1;

            while (i >= 0)
            {
                builder.Append((value & (1 << i)) == 0 ? 0 : 1);
                i--;
            }
        }

        /// <summary>
        /// Formats an integer value to a binary string.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <param name="digits">The number of digits to include in the string.</param>
        /// <returns>A binary string.</returns>
        public static string ToBinaryString(uint value, int digits = 32)
        {
            var sb = new StringBuilder(digits);
            ToBinaryString(value, sb, digits);
            return sb.ToString();
        }

        /// <summary>
        /// Converts a collection of bool values to a bit mask.
        /// </summary>
        /// <param name="collection">The collection of bool values.</param>
        /// <returns>Binary mask.</returns>
        public static uint ToBinaryMask(IEnumerable<bool> collection)
        {
            uint mask = 0;
            var count = 0;
            foreach (bool value in collection)
            {
                if (value) mask |= (uint) (1 << count);
                if (++count >= 32) break;
            }
            return mask;
        }
    }
}