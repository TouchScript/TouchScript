/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Collections.Generic;
using System.Text;

namespace TouchScript.Utils
{
    public static class BinaryUtils
    {
        public static void ToBinaryString(uint value, StringBuilder builder, int digits = 32)
        {
            int i = digits - 1;

            while (i >= 0)
            {
                builder.Append((value & (1 << i)) == 0 ? 0 : 1);
                i--;
            }
        }

        public static string ToBinaryString(uint value, int digits = 32)
        {
            var sb = new StringBuilder(digits);
            ToBinaryString(value, sb, digits);
            return sb.ToString();
        }

        public static uint ToBinaryMask(IEnumerable<bool> collection)
        {
            uint mask = 0;
            var count = 0;
            foreach (bool value in collection)
            {
                if (value) mask |= (uint)(1 << count);
                if (++count >= 32) break;
            }
            return mask;
        }
    }
}