using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;

namespace BeerBubbleUtility
{
    public sealed class AspCachingService : ICachingService
    {
        private readonly Cache cache;
        private static AspCachingService current = null;

        private const int DEFAULT_MINUTES = 3;
        private static bool enableCache = true;


        private AspCachingService()
        {
            HttpContext context = HttpContext.Current;
            if (context != null)
            {
                cache = context.Cache;
            }
            else
            {
                cache = HttpRuntime.Cache;
            }
            if (System.Configuration.ConfigurationManager.AppSettings["EnableCache"] != null)
            {
                enableCache = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["EnableCache"]);
            }
        }

        public static AspCachingService Current
        {
            get
            {
                if (current == null)
                {
                    current = new AspCachingService();
                }
                return current;

            }
        }

        public string Name
        {
            get
            {
                return "AspCachingService";
            }
        }

        public int Count
        {
            get
            {
                return cache.Count;
            }
        }

        public Hashtable State
        {
            get
            {
                Hashtable cacheHashtable = new Hashtable();
                IDictionaryEnumerator cacheEnum = cache.GetEnumerator();
                while (cacheEnum.MoveNext())
                {
                    cacheHashtable.Add(cacheEnum.Key, cacheEnum.Value);
                }
                return cacheHashtable;
            }
        }

        public object Get(string key)
        {
            if (!enableCache)
                return null;
            return cache[key];
        }

        public void Remove(string key)
        {
            cache.Remove(key);
        }

        /// <summary>
        /// 返回需要被Memcached移除的缓存key
        /// </summary>
        /// <param name="keyPattern"></param>
        /// <returns></returns>
        public List<string> RemoveByPattern(string keyPattern, bool returnMatchKeys)
        {
            List<string> matchKeys = new List<string>();
            string[] keyPatterns = keyPattern.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
            IDictionaryEnumerator cacheEnum = cache.GetEnumerator();
            while (cacheEnum.MoveNext())
            {
                string key = cacheEnum.Key.ToString();
                if (!key.EndsWith("_token"))
                {
                    foreach (string pattern in keyPatterns)
                    {
                        if (key.StartsWith(pattern, StringComparison.OrdinalIgnoreCase))
                        {
                            cache.Remove(key);
                            if (returnMatchKeys)
                            {
                                matchKeys.Add(key);
                            }
                            continue;
                        }
                    }
                }
            }
            return matchKeys;
        }

        public void Clear()
        {
            IDictionaryEnumerator cacheEnum = cache.GetEnumerator();
            StringCollection cacheKeys = new StringCollection();
            while (cacheEnum.MoveNext())
            {
                cacheKeys.Add(cacheEnum.Key.ToString());
            }
            for (int i = 0, count = cacheKeys.Count; i < count; i++)
            {
                Remove(cacheKeys[i]);
            }
        }

        public void Add(string key, object value)
        {
            Add(key, value, DEFAULT_MINUTES);
        }

        public void MicroInsert(string key, object value, int seconds)
        {
            Add(key, value, DateTime.Now.AddSeconds(seconds));
        }

        public void MicroInsert(string key, object value, int seconds, string filePath)
        {
            Add(key, value, filePath, DateTime.Now.AddSeconds(seconds), null);
        }

        public void Add(string key, object value, int minutes)
        {
            Add(key, value, DateTime.Now.AddMinutes(minutes));
        }

        public void Add(string key, object value, int minutes, OnCacheRemoved onCacheRemoved)
        {
            Add(key, value, DateTime.Now.AddMinutes(minutes), onCacheRemoved);
        }

        public void Add(string key, object value, DateTime dateTime)
        {
            Add(key, value, dateTime, null);
        }

        public void Add(string key, object value, DateTime dateTime, OnCacheRemoved onCacheRemoved)
        {
            Add(key, value, string.Empty, dateTime, onCacheRemoved);
        }

        public void Add(string key, object value, string filepath)
        {
            Add(key, value, filepath, null);
        }

        public void Add(string key, object value, string filepath, OnCacheRemoved onCacheRemoved)
        {
            Add(key, value, filepath, DateTime.Now.AddDays(1), onCacheRemoved);
        }

        public void Add(string key, object value, string filepath, DateTime dateTime, OnCacheRemoved onCacheRemoved)
        {
            if (!enableCache)
                return;
            
            CacheDependency dep = null;
            if (!string.IsNullOrEmpty(filepath))
            {
                dep = new CacheDependency(filepath);
            }
            
            CacheItemRemovedCallback onRemoveCallback = null;
            if (onCacheRemoved != null)
            {
                AspCachingOnRemove aspCachingOnRemove = new AspCachingOnRemove();
                onRemoveCallback = new CacheItemRemovedCallback(aspCachingOnRemove.RemovedCallback);
                aspCachingOnRemove.RemoveEvent += new AspCachingOnRemove.RemoveHandler(onCacheRemoved.RemoveHandler);
            }
            
            cache.Insert(key, value, dep, dateTime, TimeSpan.Zero, CacheItemPriority.NotRemovable, onRemoveCallback);
        }

    }
}
