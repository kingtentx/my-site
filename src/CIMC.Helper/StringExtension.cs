using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;

namespace CIMC.Helper
{
    /// <summary>提供字符串和集合的扩展方法。</summary>
    public static class StringExtension
    {
        /// <summary>
        /// 截取字符串，多余部分用"..."代替
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <param name="length">截取长度</param>
        /// <returns></returns>
        public static string CutString(this string str, int length)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (str.Length <= length)
                {
                    return str;
                }
                else
                {
                    return str.Substring(0, length) + "...";
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>移除字符串末尾的指定内容。</summary>
        public static string RemovePostFix(this string str, params string[] postFixes)
        {
            return str.RemovePostFix(StringComparison.Ordinal, postFixes);
        }

        /// <summary>移除字符串末尾的指定内容。</summary>
        public static string RemovePostFix(this string str, StringComparison comparisonType, params string[] postFixes)
        {
            if (str.IsNullOrEmpty())
            {
                return str;
            }

            if (postFixes.IsNullOrEmpty())
            {
                return str;
            }

            foreach (string text in postFixes)
            {
                if (str.EndsWith(text, comparisonType))
                {
                    return str.Left(str.Length - text.Length);
                }
            }

            return str;
        }

        /// <summary>判断字符串或集合是否为空。</summary>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        /// <summary>判断字符串或集合是否为空。</summary>
        public static bool IsNullOrEmpty<T>(this ICollection<T> source)
        {
            if (source != null)
            {
                return source.Count <= 0;
            }

            return true;
        }

        /// <summary>截取字符串左侧指定长度的内容。</summary>
        public static string Left(this string str, int len)
        {
            if (str == null)
            {
                throw new ArgumentNullException("string is null");
            }
            if (str.Length < len)
            {
                throw new ArgumentException("len argument can not be bigger than given string's length!");
            }

            return str.Substring(0, len);
        }
    }
}
