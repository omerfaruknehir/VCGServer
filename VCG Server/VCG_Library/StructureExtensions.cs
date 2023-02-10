using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCG_Library
{
    public static class StructureExtensions
    {
        //  Array
        public static bool Remove<T>(this T[] array, T item)
        {
            int index = 0;
            foreach (T i in array)
            {
                if (item.Equals(i))
                {
                    array.SetValue(null, index);
                    return true;
                }
                index++;
            }
            return false;
        }
        //  Array

        //  Char
        public static bool IsInt(this char chr)
        {
            return chr == '0' || chr == '1' || chr == '2' || chr == '3' || chr == '4' || chr == '5' || chr == '6' || chr == '7' || chr == '8' || chr == '9';
        }
        //  Char


        //  String
        public static int[] GetIntegers(this string str)
        {
            List<string> rawResult = new List<string>();

            int rri = -1;

            bool lI = false;

            foreach (char chr in str)
            {
                if (chr.IsInt())
                {
                    if (lI)
                    {
                        rawResult[rri] += chr;
                    }
                    else
                    {
                        rawResult.Add(chr.ToString());
                    }
                    lI = true;
                }
                else
                {
                    lI = false;
                }
            }

            int[] result = new int[rawResult.Count];

            int i = 0;
            foreach (string strng in rawResult)
            {
                result[i++] = Int32.Parse(strng);
            }

            return result;
        }

        public static string ReverseString(this string str)
        {
            string res = "";
            for (int i = str.Length - 1; i >= 0; i--)
            {
                res += str[i];
            }
            return res;
        }

        public static bool TryParseEnd(this string str, out int end)
        {
            if (!str[str.Length].IsInt())
            {
                end = 0;
                return false;
            }

            string parsed = "";
            for (int index = str.Length - 1; index > 0; index--)
            {
                if (str[index].IsInt())
                {
                    parsed += str[index];
                }
                else
                {
                    break;
                }
            }
            end = Int32.Parse(parsed.Reverse().ToString());
            return true;
        }

        public static int ParseEnd(this string str, out int end)
        {
            if (!str[str.Length - 1].IsInt())
            {
                end = 0;
                return 0;
            }

            int l = 0;
            string parsed = "";
            for (int index = str.Length - 1; index > 0; index--)
            {
                if (str[index].IsInt())
                {
                    parsed += str[index];
                    l += 1;
                }
                else
                {
                    break;
                }
            }
            end = Int32.Parse(parsed.ReverseString());
            return l;
        }
        //  String
    }
}
