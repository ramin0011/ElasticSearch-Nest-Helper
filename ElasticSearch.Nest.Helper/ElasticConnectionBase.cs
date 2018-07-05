using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;

namespace ElasticSearch.Nest.Helper
{
    public class ElasticConnectionBase
    {
        protected string DefaultIndex;
        protected static string indexPrefix;
        public ElasticClient _elasticClient;
        protected static string dbName;
        protected string nodeUrl;
        protected string userName;
        protected string password;
        protected static Dictionary<Type, string> TypeTable = new Dictionary<Type, string>();

        public static void SetToProductionMode()
        {
            indexPrefix = $"{dbName}-{ElasticConnectionIndexPrefixes.Production}";
        }
        public static void SetToTestMode()
        {
            indexPrefix = $"{dbName}-{ElasticConnectionIndexPrefixes.Test}";
        }

        protected ElasticClient GetDb<T>()
        {
            this.DefaultIndex = TypeTable.Single(a => a.Key == typeof(T)).Value;
            this.DefaultIndex = $"{indexPrefix}{DefaultIndex.ToLower()}";
            var uriString = this.nodeUrl;
            var node = new Uri(uriString);
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex(DefaultIndex);
            var userName = this.userName;
            if (!string.IsNullOrEmpty(userName))
                settings.
                    BasicAuthentication(userName, this.password);
#if DEBUG
            settings.EnableDebugMode();
#endif
            var client = new ElasticClient(settings);
            return _elasticClient = client;
        }
        protected ElasticClient GetDb(string index)
        {
            if (TypeTable.Count == 0)
                throw new Exception("Elastic Search Mapping Is Not Defined Call The Init Method First");
            this.DefaultIndex = $"{index.ToLower()}";
            var uriString = this.nodeUrl;
            var node = new Uri(uriString);
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex(DefaultIndex);
            var userName = this.userName;
            if (!string.IsNullOrEmpty(userName))
                settings.
                    BasicAuthentication(userName, this.password);
#if DEBUG
            settings.EnableDebugMode();
#endif
            var client = new ElasticClient(settings);
            return _elasticClient = client;
        }
    }
}
