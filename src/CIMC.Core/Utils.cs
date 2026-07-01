using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CIMC.Core
{
    public class Utils
    {

        /// <summary>
        /// POST文件上传
        /// </summary>
        /// <param name="getUrl"></param>
        /// <param name="sm"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static bool PostFile(string getUrl, Stream sm, string filename)
        {
            Stream fileStream = sm;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls;//https形式需要添加
            bool returnValue = false;
            fileStream.Position = 0;
            var r = new BinaryReader(fileStream);
            string strBoundary = "--" + DateTime.Now.Ticks.ToString("x");
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + strBoundary + "--\r\n");
            //请求头部信息
            StringBuilder sb = new StringBuilder();
            sb.Append("--");
            sb.Append(strBoundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"importType\"");
            sb.Append("\r\n\r\n1");
            sb.Append("\r\n");
            sb.Append("--");
            sb.Append(strBoundary);
            sb.Append("\r\n");
            sb.Append("Content-Disposition: form-data; name=\"file\"; filename=\"" + filename + "\"");
            sb.Append("\r\n");
            sb.Append("Content-Type: ");
            sb.Append("application/octet-stream");
            sb.Append("\r\n\r\n");
            string strPostHeader = sb.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(strPostHeader);
            try
            {
                // 根据uri创建HttpWebRequest对象
                HttpWebRequest httpReq = (HttpWebRequest)WebRequest.Create(new Uri(getUrl));
                httpReq.Method = "POST";
                //对发送的数据不使用缓存
                httpReq.AllowWriteStreamBuffering = false;
                //设置获得响应的超时时间（300秒）
                httpReq.Timeout = 300000;
                //httpReq.ServicePoint.Expect100Continue = false;
                //httpReq.CookieContainer = cookieContainer;
                //httpReq.Accept = header.accept;
                //httpReq.Headers.Add("Accept-Encoding","gzip, deflate, br");
                //httpReq.Headers.Add("TE", "Trailers");
                //httpReq.UserAgent = header.userAgent;
                httpReq.ContentType = "multipart/form-data; boundary=" + strBoundary;
                long length = fileStream.Length + postHeaderBytes.Length + boundaryBytes.Length;
                long fileLength = fileStream.Length;
                httpReq.ContentLength = length;
                byte[] buffer = new byte[fileLength];
                Stream postStream = httpReq.GetRequestStream();
                //发送请求头部消息
                postStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
                int size = r.Read(buffer, 0, buffer.Length);
                postStream.Write(buffer, 0, size);
                //添加尾部的时间戳
                postStream.Write(boundaryBytes, 0, boundaryBytes.Length);
                postStream.Close();
                //获取服务器端的响应
                HttpWebResponse webRespon = (HttpWebResponse)httpReq.GetResponse();
                if (webRespon.StatusCode == HttpStatusCode.OK) //如果服务器未响应，那么继续等待相应                 
                {
                    Stream s = webRespon.GetResponseStream();
                    StreamReader sr = new StreamReader(s);
                    //读取服务器端返回的消息
                    string sReturnString = sr.ReadLine();
                    //Log.Information(sReturnString);
                    s.Close();
                    sr.Close();
                    fileStream.Close();
                    returnValue = true;
                }
            }
            catch (Exception ex)
            {
                //Log.Error(string.Format("文件上传失败：{0}", ex.Message));
            }
            return returnValue;
        }

        /// <summary>
        /// Get请求获取url地址输出内容,Encoding.UTF8编码
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string GetDownloadString(string url)
        {
            var client = new System.Net.WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            return client.DownloadString(url);
        }

        /// <summary>   
        /// Get请求获取url地址输出内容   
        /// </summary> 
        /// <param name="url">url</param>   
        /// <param name="encoding">返回内容编码方式，例如：Encoding.UTF8</param>   
        public static string GetWeb(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.Timeout = 30 * 1000;
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            StreamReader sr = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// Get请求获取url地址输出流
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Stream GetStream(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.Timeout = 30 * 1000;
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            Stream sr = webResponse.GetResponseStream();
            return sr;
        }

        /// <summary>
        /// Get请求获取url地址输出
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpWebResponse GetWebResponse(string url)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Method = "GET";
            webRequest.Timeout = 30 * 1000;
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
            return webResponse;
        }

        /// <summary>
        /// Post请求获取url地址输出内容
        /// </summary>
        public static string PostWeb(string url, string postContent)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.Timeout = 30 * 1000;
            request.ContentType = "application/x-www-form-urlencoded";

            Encoding encoding = Encoding.GetEncoding("utf-8");
            if (!string.IsNullOrEmpty(postContent))
            {
                Stream stream = request.GetRequestStream();
                byte[] dataMenu = encoding.GetBytes(postContent);
                stream.Write(dataMenu, 0, dataMenu.Length);
                stream.Flush();
                stream.Close();
            }
            WebResponse response = request.GetResponse();
            Stream inStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(inStream);
            return sr.ReadToEnd();
        }

        /// <summary>
        /// Post请求获取url地址输出内容
        /// </summary>
        public static string PostWeb(string url, string postContent, string appId, string nonce, string timestamp, string sign)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.Timeout = 30 * 1000;
            request.ContentType = "application/json";
            request.Headers.Add("appId", appId);
            request.Headers.Add("nonce", nonce);
            request.Headers.Add("timestamp", timestamp);
            request.Headers.Add("sign", sign);

            Encoding encoding = Encoding.GetEncoding("utf-8");
            if (!string.IsNullOrEmpty(postContent))
            {
                Stream stream = request.GetRequestStream();
                byte[] dataMenu = encoding.GetBytes(postContent);
                stream.Write(dataMenu, 0, dataMenu.Length);
                stream.Flush();
                stream.Close();
            }
            WebResponse response = request.GetResponse();
            Stream inStream = response.GetResponseStream();
            StreamReader sr = new StreamReader(inStream);
            return sr.ReadToEnd();
        }

    }
}
