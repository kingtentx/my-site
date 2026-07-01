using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CIMC.Helper
{
    public class StringHelper
    {
        /// <summary>
        /// 压缩html代码
        /// </summary>
        /// <param name="text">html代码</param>
        /// <returns></returns>
        public static string CompressHtml(string text)
        {
            var reg = new Regex(@"\s*(</?[^\s/>]+[^>]*>)\s+(</?[^\s/>]+[^>]*>)\s*");
            text = reg.Replace(text, m => m.Groups[1].Value + m.Groups[2].Value);

            //移除html标签之间的空格 
            text = new Regex(@"(?<=>)[\s|\n|\t]*(?=<)").Replace(text, string.Empty);

            //移除多余空格和换行符
            text = new Regex(@"\n+\s+").Replace(text, string.Empty);

            return text;
        }

        public static bool IsSafeSqlString(string str)
        {
            return !Regex.IsMatch(str, "[-|;|,|\\/|\\(|\\)|\\[|\\]|\\}|\\{|%|@|\\*|!|\\']");
        }
        public static string RemoveHtml(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }
            string pattern = "<[^>]*>";
            return Regex.Replace(content, pattern, string.Empty, RegexOptions.IgnoreCase).Trim();
        }

        /// <summary>
        /// C#反射遍历ASCII排序对象属性 并过滤sign属性字段
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="model">对象</param>
        public static SortedDictionary<string, object> ForeachPropertiesToDictionary<T>(T model, string strSign = "sign")
        {
            SortedDictionary<string, object> dic = new SortedDictionary<string, object>();
            Type t = model.GetType();
            PropertyInfo[] PropertyList = t.GetProperties();
            foreach (PropertyInfo item in PropertyList)
            {
                if (item.Name.ToLower() == strSign)
                {
                    continue;
                }
                object value = item.GetValue(model, null);
                if (value != null)
                    dic.Add(item.Name, value);
            }
            return dic;
        }

        /// <summary>
        /// 拼接 &key=value 格式字符串
        /// </summary>
        /// <param name="dicList"></param>
        /// <returns></returns>
        public static string SpliceString(SortedDictionary<string, object> dic)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in dic)
            {
                sb.Append(item.Key + "=" + item.Value + "&");
            }
            return sb.ToString().TrimEnd('&');
        }

        /// <summary> 
        /// 获取10位时间戳
        /// </summary> 
        /// <returns></returns> 
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds);
        }

        /// <summary>  
        /// 获取13位时间戳 DateTime时间格式转换为Unix时间戳格式  
        /// </summary>        
        /// <returns>long</returns>  
        public static long GetUnixTimeStamp()
        {           
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (DateTime.Now.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }

        /// <summary>
        ///  MD5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToMD5(string str)
        {
            if (str == null)
            {
                str = string.Empty;
            }
            MD5 md5 = MD5.Create();
            byte[] bytes_out = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            string result = BitConverter.ToString(bytes_out).Replace("-", "");
            return result;
        }

        /// <summary>
        ///  SHA1加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToSHA1(string str)
        {
            if (str == null)
            {
                str = string.Empty;
            }
            SHA1 sha1 = SHA1.Create();
            byte[] bytes_out = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
            string result = BitConverter.ToString(bytes_out).Replace("-", "");
            return result;
        }

        /// <summary>
        ///  SHA256加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToSHA256(string str)
        {
            if (str == null)
            {
                str = string.Empty;
            }

            SHA256 sha1 = SHA256.Create();
            byte[] bytes_out = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
            string result = BitConverter.ToString(bytes_out).Replace("-", "");
            return result.ToLower();
        }

        /// <summary>
        /// 添加URL参数
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AddUrlParameter(string url, string key, string value)
        {
            int num = url.LastIndexOf("#");
            string str = string.Empty;
            if (num > -1)
            {
                str = url.Substring(num);
                url = url.Substring(0, num);
            }
            int num2 = url.IndexOf("?");
            if (num2 < 0)
            {
                string text = url;
                url = string.Concat(new string[]
                {
                    text,
                    "?",
                    key,
                    "=",
                    value
                });
            }
            else
            {
                Regex regex = new Regex("(?<=[&\\?])" + key + "=[^\\s&#]*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                if (regex.IsMatch(url))
                {
                    url = regex.Replace(url, key + "=" + value);
                }
                else
                {
                    string text2 = url;
                    url = string.Concat(new string[]
                    {
                        text2,
                        "&",
                        key,
                        "=",
                        value
                    });
                }
            }
            return url + str;
        }
        /// <summary>
        /// 移除URL参数
        /// </summary>
        /// <param name="url"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string RemoveUrlParameter(string url, string key)
        {
            Regex regex = new Regex("[&\\?]" + key + "=[^\\s&#]*&?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            return regex.Replace(url, new MatchEvaluator(PutAwayGarbageFromUrl));
        }
        private static string PutAwayGarbageFromUrl(Match match)
        {
            string value = match.Value;
            if (value.EndsWith("&"))
            {
                return value.Substring(0, 1);
            }
            return string.Empty;
        }

        public static bool IsTime(string timeval)
        {
            return !string.IsNullOrEmpty(timeval) && Regex.IsMatch(timeval, "^((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?)$");
        }
        public static bool IsInt(string str)
        {
            return !string.IsNullOrEmpty(str) && Regex.IsMatch(str, "^[0-9]*$");
        }
        public static bool IsNumeric(string str)
        {
            return str != null && str.Length > 0 && str.Length <= 11 && Regex.IsMatch(str, "^[-]?[0-9]*[.]?[0-9]*$") && (str.Length < 10 || (str.Length == 10 && str[0] == '1') || (str.Length == 11 && str[0] == '-' && str[1] == '1'));
        }
        public static bool IsNumericArray(string[] strNumber)
        {
            if (strNumber == null)
            {
                return false;
            }
            if (strNumber.Length < 1)
            {
                return false;
            }
            for (int i = 0; i < strNumber.Length; i++)
            {
                string str = strNumber[i];
                if (!StringHelper.IsNumeric(str))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsDouble(string expression)
        {
            return expression != null && Regex.IsMatch(expression, "^([0-9])[0-9]*(\\.\\w*)?$");
        }
        public static bool IsUrl(string strUrl)
        {
            return !string.IsNullOrEmpty(strUrl) && (strUrl.StartsWith("http://") || strUrl.StartsWith("https://"));
        }
        public static bool IsEmail(string strEmail)
        {
            return !string.IsNullOrEmpty(strEmail) && Regex.IsMatch(strEmail, "^([\\w-\\.]+)@((\\[[0-9]{1,3}\\.[0-9]{1,3}\\.[0-9]{1,3}\\.)|(([\\w-]+\\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\\]?)$");
        }
        /// <summary>
        /// 电话&手机
        /// </summary>
        /// <param name="strTelphone"></param>
        /// <returns></returns>
        public static bool IsTelphone(string strTelphone)
        {
            return !string.IsNullOrEmpty(strTelphone) && Regex.IsMatch(strTelphone, "\\d{3,4}-\\d{7,8}$|^1[3456789]\\d{9}$");
        }
        public static bool IsPhoneNumber(string strPhoneNumber)
        {
            return !string.IsNullOrEmpty(strPhoneNumber) && Regex.IsMatch(strPhoneNumber, "^(13[0-9]|14[01456879]|15[0-35-9]|16[2567]|17[0-8]|18[0-9]|19[0-35-9])\\d{8}$");
        }
        public static bool IsIPAddress(string ipAddress)
        {
            return !string.IsNullOrEmpty(ipAddress) && Regex.IsMatch(ipAddress, "^((2[0-4]\\d|25[0-5]|[01]?\\d\\d?)\\.){3}(2[0-4]\\d|25[0-5]|[01]?\\d\\d?)$");
        }
        public static int ToInt(string str, int def)
        {
            if (string.IsNullOrEmpty(str) || str.Trim().Length >= 11 || !Regex.IsMatch(str.Trim(), "^([-]|[0-9])[0-9]*(\\.\\w*)?$"))
            {
                return def;
            }
            int result;
            if (int.TryParse(str, out result))
            {
                return result;
            }
            return def;
        }
        public static decimal ToDecimal(string str, decimal def)
        {
            decimal result;
            try
            {
                result = decimal.Parse(str);
            }
            catch
            {
                result = def;
            }
            return result;
        }
        public static long ToInt64(string str, long def)
        {
            long result;
            if (long.TryParse(str, out result))
            {
                return result;
            }
            return def;
        }
        public static float ToFloat(string strValue, float defValue)
        {
            if (strValue == null || strValue.Length > 10)
            {
                return defValue;
            }
            float result = defValue;
            if (strValue != null)
            {
                bool flag = Regex.IsMatch(strValue, "^([-]|[0-9])[0-9]*(\\.\\w*)?$");
                if (flag)
                {
                    float.TryParse(strValue, out result);
                }
            }
            return result;
        }
        /// <summary>
        /// 获取随机字符串
        /// </summary>
        /// <param name="length"></param>
        /// <param name="isLower"></param>
        /// <returns></returns>
        public static string RandCode(int length, bool isLower = false)
        {
            char[] array = new char[]
            {
              '0','1','2','3','4','5','6','7','8','9','a','b','d','c','e','f','g','h','i','j','k','l','m','n','p','r','q','s','t','u','v','w','z','y','x','0','1','2','3','4','5','6','7','8','9','A','B','C','D','E','F','G','H','I','J','K','L','M','N','Q','P','R','T','S','V','U','W','X','Y','Z'
            };
            StringBuilder stringBuilder = new StringBuilder();
            Random random = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append(array[random.Next(0, array.Length)].ToString());
            }
            if (isLower)
            {
                return stringBuilder.ToString().ToLower();
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 字符串转Unicode 
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>Unicode编码后的字符串</returns>
        public static string StringToUnicode(string str)
        {

            byte[] bytes = Encoding.Unicode.GetBytes(str);
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("\\u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Unicode转字符串
        /// </summary>
        /// <param name="str">经过Unicode编码的字符串</param>
        /// <returns>正常字符串</returns>
        public static string UnicodeToString(string str)
        {
            //source = RemoveEmoji(source);
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                         str, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));
        }
    }
}
