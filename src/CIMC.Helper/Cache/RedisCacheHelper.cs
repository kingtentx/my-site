using Microsoft.Extensions.Caching.StackExchangeRedis;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CIMC.Helper
{
    public class RedisCacheHelper : ICacheService
    {
        protected IDatabase _database;
        private ConnectionMultiplexer _connection;
        private string _instance;
        public RedisCacheHelper(RedisCacheOptions options)
        {
            _connection = ConnectionMultiplexer.Connect(options.Configuration);
            _instance = options.InstanceName;
            _database = _connection.GetDatabase(GetDatabaseIndex(options.Configuration));
        }

        private int GetDatabaseIndex(string configuration, int defaultDatabase = 0)
        {
            const string dbKeys = "defaultdatabase=";
            int index = configuration.ToLower().IndexOf(dbKeys);
            if (index != -1)
            {
                int startIndex = index + dbKeys.Length;
                int endIndex = configuration.IndexOf(',', startIndex);
                string dbIndexString = endIndex == -1 ? configuration.Substring(startIndex) : configuration.Substring(startIndex, endIndex - startIndex);
                if (int.TryParse(dbIndexString, out int dbIndex))
                {
                    return dbIndex;
                }
            }
            return defaultDatabase;
        }


        /// <summary>
        /// 验证缓存项是否存在
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _database.KeyExists(GetKeyForRedis(key));
        }

        #region 添加缓存

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <returns></returns>
        public bool Add(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            return _database.StringSet(GetKeyForRedis(key), Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)));
        }

        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <param name="expiresSliding">滑动过期时长（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <param name="expiressAbsoulte">绝对过期时长</param>
        /// <returns></returns>
        public bool Add(string key, object value, TimeSpan expiresSliding, TimeSpan expiressAbsoulte)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            var serializedValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
            // 设置值并设置绝对过期时间  
            var isSet = _database.StringSet(GetKeyForRedis(key), serializedValue, expiressAbsoulte);
            // 如果设置成功，并且需要滑动过期  
            if (isSet && expiresSliding > TimeSpan.Zero)
            {
                // 使用 KeyExpire 方法设置滑动过期时间  
                return _database.KeyExpire(GetKeyForRedis(key), expiresSliding);
            }
            return isSet;
        }
        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">缓存Value</param>
        /// <param name="expiresIn">缓存时长</param>
        /// <param name="isSliding">是否滑动过期（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <returns></returns>
        public bool Add(string key, object value, TimeSpan expiresIn, bool isSliding = false)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            var serializedValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value));
            // 如果不是滑动过期，直接设置过期时间  
            if (!isSliding)
            {
                return _database.StringSet(GetKeyForRedis(key), serializedValue, expiresIn);
            }
            // 如果是滑动过期，先设置值，不设置过期时间  
            var isSet = _database.StringSet(GetKeyForRedis(key), serializedValue);
            if (isSet)
            {
                // 使用 KeyExpire 方法设置过期时间  
                return _database.KeyExpire(GetKeyForRedis(key), expiresIn);
            }
            return false;
        }

        #endregion

        #region 获取缓存
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public T Get<T>(string key) where T : class
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var value = _database.StringGet(GetKeyForRedis(key));

            if (!value.HasValue)
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(value);

        }
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public object Get(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            var value = _database.StringGet(GetKeyForRedis(key));

            if (!value.HasValue)
            {
                return null;
            }
            return JsonConvert.DeserializeObject(value);
        }

        /// <summary>
        /// 获取缓存集合
        /// </summary>
        /// <param name="keys">缓存Key集合</param>
        /// <returns></returns>
        public IDictionary<string, object> GetAll(IEnumerable<string> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }
            var dict = new Dictionary<string, object>();

            keys.ToList().ForEach(item => dict.Add(item, Get(GetKeyForRedis(item))));

            return dict;
        }

        #endregion

        #region 删除缓存
        /// <summary>
        /// 删除缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <returns></returns>
        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _database.KeyDelete(GetKeyForRedis(key));
        }
        /// <summary>
        /// 批量删除缓存
        /// </summary>
        /// <param name="key">缓存Key集合</param>
        /// <returns></returns>
        public void RemoveAll(IEnumerable<string> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException(nameof(keys));
            }

            keys.ToList().ForEach(item => Remove(GetKeyForRedis(item)));
        }

        /// <summary>  
        /// 根据前缀删除缓存  
        /// </summary>  
        /// <param name="prefix">缓存Key前缀</param>  
        public void RemoveByPrefix(string prefix)
        {
            if (prefix == null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }

            var server = _connection.GetServer(_connection.GetEndPoints().First());
            var keys = server.Keys(_database.Database, GetKeyForRedis(prefix) + "*").ToArray();

            if (keys.Length > 0)
            {
                _database.KeyDelete(keys);
            }
        }
        #endregion

        #region 修改缓存
        /// <summary>
        /// 修改缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">新的缓存Value</param>
        /// <returns></returns>
        public bool Replace(string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (Exists(key))
                if (!Remove(key))
                    return false;

            return Add(key, value);

        }
        /// <summary>
        /// 修改缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">新的缓存Value</param>
        /// <param name="expiresSliding">滑动过期时长（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <param name="expiressAbsoulte">绝对过期时长</param>
        /// <returns></returns>
        public bool Replace(string key, object value, TimeSpan expiresSliding, TimeSpan expiressAbsoulte)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (Exists(key))
                if (!Remove(key))
                    return false;

            return Add(key, value, expiresSliding, expiressAbsoulte);
        }
        /// <summary>
        /// 修改缓存
        /// </summary>
        /// <param name="key">缓存Key</param>
        /// <param name="value">新的缓存Value</param>
        /// <param name="expiresIn">缓存时长</param>
        /// <param name="isSliding">是否滑动过期（如果在过期时间内有操作，则以当前时间点延长过期时间）</param>
        /// <returns></returns>
        public bool Replace(string key, object value, TimeSpan expiresIn, bool isSliding = false)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (Exists(key))
                if (!Remove(key)) return false;

            return Add(key, value, expiresIn, isSliding);
        }
        #endregion

        private string GetKeyForRedis(string key)
        {
            if (string.IsNullOrEmpty(_instance))
                return key;
            else
                return _instance + ":" + key;
        }
        public void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();
            GC.SuppressFinalize(this);
        }


    }
}
