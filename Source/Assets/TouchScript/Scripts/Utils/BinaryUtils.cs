/*
 * @author Valentin Simonov / http://va.lent.in/
 */

using System.Text;

namespace TouchScript.Utils
{
    public static class BinaryUtils
    {

        public static void ToBinaryString(uint value, StringBuilder builder, int digits = 32)
        {
            int i = digits-1;

            while (i >= 0)
            {
                builder.Append((value & (1 << i)) == 0 ? 0 : 1);
                i--;
            }
        }

    }
}
