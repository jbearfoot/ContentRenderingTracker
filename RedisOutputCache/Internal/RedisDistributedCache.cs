using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RedisOutputCache.Internal
{
    public class RedisDitributedCache : IDistributedCache
    {
        Lazy<ConnectionMultiplexer> _lazyConnection;
        public RedisDitributedCache(string uri)
        {
            _lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                var connection = ConnectionMultiplexer.Connect($"{uri},abortConnect=false");
                connection.PreserveAsyncOrder = false;
                return connection;
            });
        }

        public Lazy<ConnectionMultiplexer> LazyConnection { get => _lazyConnection; set => _lazyConnection = value; }

        private IDatabase Redis => _lazyConnection.Value.GetDatabase();

        public void AddDependency(string key, string dependencyKey) => Redis.SetAdd(key, dependencyKey);

        public string Get(string key) => Redis.StringGet(key);

        public IEnumerable<string> GetDependencies(string key) => Redis.SetMembers(key).Select(rv => (string)rv);

        public void Remove(IEnumerable<string> keys) => Redis.KeyDelete(keys.Select(k => (RedisKey)k).ToArray());

        public void Set(string key, string value) => Redis.StringSet(key, value);
    }
}
