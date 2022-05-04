using System;
using System.Collections.Specialized;
using System.Runtime.Caching;
using ToolGood.Words;
using Wox.Infrastructure.UserSettings;

namespace Wox.Infrastructure
{
    public class Alphabet
    {
        private Settings _settings;
        private MemoryCache _cache;

        public void Initialize()
        {
            _settings = Settings.Instance;
            NameValueCollection config = new NameValueCollection
            {
                { "pollingInterval", "00:05:00" },
                { "physicalMemoryLimitPercentage", "1" },
                { "cacheMemoryLimitMegabytes", "30" }
            };
            _cache = new MemoryCache("AlphabetCache", config);
        }

        public string Translate(string content)
        {
            if (_settings.ShouldUsePinyin)
            {
                if (!(_cache[content] is string result))
                {
                    if (WordsHelper.HasChinese(content))
                    {
                        result = WordsHelper.GetFirstPinyin(content);
                    }
                    else
                    {
                        result = content;
                    }
                    CacheItemPolicy policy = new CacheItemPolicy
                    {
                        SlidingExpiration = new TimeSpan(12, 0, 0)
                    };
                    _cache.Set(content, result, policy);
                }
                return result;
            }
            else
            {
                return content;
            }
        }
    }
}
