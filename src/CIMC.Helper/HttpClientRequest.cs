using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CIMC.Helper
{
    /// <summary>
    /// 基于HttpClient封装的请求类
    /// </summary>
    public class HttpClientRequest
    {
        private static readonly HttpClient HttpClient;

        static HttpClientRequest()
        {
            HttpClient = new HttpClient();
        }

        /// <summary>
        /// 异步 Get 一个请求
        /// </summary>
        /// <param name="requestUri">请求URL</param>       
        /// <returns></returns>
        public static async Task<string> GetAsync(string requestUri)
        {
            var response = await HttpClient.GetAsync(requestUri);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }

        /// <summary>
        /// 异步 Get 一个请求
        /// </summary>
        /// <param name="requestUri">请求URL</param>
        /// <param name="urlDictionary">请求参数</param>
        /// <returns></returns>
        public static async Task<string> GetAsync(string requestUri, Dictionary<string, string> urlDictionary)
        {
            var paramter = urlDictionary.Aggregate(string.Empty, (current, item) => current + (item.Key + "=" + item.Value + "&"));
            var response = await HttpClient.GetAsync(requestUri + "?" + paramter.TrimEnd('&'));
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }


        /// <summary>
        /// 异步Post 一个请求
        /// </summary>
        /// <param name="requestUri">请求URL</param>
        /// <param name="urlDictionary">请求参数</param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string requestUri, Dictionary<string, string> urlDictionary)
        {
            var paramter = new FormUrlEncodedContent(urlDictionary);
            var response = await HttpClient.PostAsync(requestUri, paramter);
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// 异步Post 一个请求
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="strPost"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static async Task<string> PostAsync(string requestUri, string strPost, string contentType = "application/json")
        {
            var content = new StringContent(strPost);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            var response = await HttpClient.PostAsync(requestUri, content);
            return await response.Content.ReadAsStringAsync();
        }
    }
}