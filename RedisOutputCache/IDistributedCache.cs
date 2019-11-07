using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisOutputCache
{
    public interface IDistributedCache
    {
        //I found that the async methods was not that stable (perhaps wrong usage of me...)
        //so I decided to use the sync approach...
        string Get(string key);
        void Set(string key, string value);
        void AddDependency(string key, string dependencyKey);
        IEnumerable<string> GetDependencies(string key);
        void Remove(IEnumerable<string> keys);
    }
}
