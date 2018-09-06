using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace ES.Helper
{
    public class ElasticConnection :ElasticConnectionBase, IDisposable, IElasticConnection
    {

        private string DefaultIndex;
        private static string indexPrefix;
        private ElasticClient _elasticClient;
        private static string NodeUrl;
        private static string UserName;
        private static string Password;
        private static string dbName;

        public static Dictionary<Type, string> TypeTable = new Dictionary<Type, string>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeTables">define mappings ,a dictionary containing Type of a class and the class table name</param>
        /// <param name="dbName">the environment , or the db name , this is just a name that will be used as a prefix ES default index</param>
        public static void Init(Dictionary<Type, string> typeTables, string dbName)
        {
            TypeTable = typeTables;
            ElasticConnection.dbName = dbName;
        }
        /// <summary>
        /// </summary>
        /// <param name="defualtIndex">index name must be lower case</param>
        /// <returns></returns>
        public ElasticConnection(string nodeUrl = null, string userName = null, string password = null)
        {

            if (string.IsNullOrEmpty(indexPrefix))
            {
                SetToTestMode();
            }

            NodeUrl = nodeUrl;
            UserName = userName;
            Password = password;
            if (string.IsNullOrEmpty(nodeUrl))
                NodeUrl = "http://127.0.0.1:9200/";
        }

        public static ElasticConnection GetLastConnection()
        {
            if (!string.IsNullOrEmpty(NodeUrl))
            {
                return new ElasticConnection(NodeUrl, UserName, Password);
            }
            return null;
        }
        public ElasticConnection GetLastDefaultEsConnection()
        {
            return ElasticConnection.GetLastConnection();
        }
        public static void SetElasticConnection(string nodeUrl, string userName = null, string password = null)
        {
            NodeUrl = nodeUrl;
            UserName = userName;
            Password = password;
            if (string.IsNullOrEmpty(nodeUrl))
                NodeUrl = "http://127.0.0.1:9200/";
        }

        public  void SetDefaultConnection(string nodeUrl, string userName = null, string password = null)
        {
            ElasticConnection.SetElasticConnection(nodeUrl,userName,password);
        }

        public string CreateQuery(string operatorWord, List<string> words)
        {
            string query = "";
            for (int i = 0; i < words.Count; i++)
            {
                query += $"({words[i]})";
                if (i != words.Count - 1)
                    query += $" {operatorWord} ";
            }

            return query;
        }
        public async Task CopyFromTo<TS>(string sourceIndex, string destinationIndex) where TS : class
        {
            GetDb(sourceIndex);
            var entities = await _elasticClient.SearchAsync<TS>(descriptor => descriptor.Size(10000));
            GetDb(destinationIndex);
            await AddOrUpdateMany<TS>(entities.Documents, true);
        }



        public static void SetToProductionMode()
        {
            indexPrefix = $"{dbName}-{ElasticConnectionIndexPrefixes.Production}";
        }
        public static void SetToTestMode()
        {
            indexPrefix = $"{dbName}-{ElasticConnectionIndexPrefixes.Test}";
        }
        private ElasticClient GetDb<T>()
        {
            this.DefaultIndex = TypeTable.Single(a => a.Key == typeof(T)).Value;
            this.DefaultIndex = $"{indexPrefix}{DefaultIndex.ToLower()}";
            var uriString = NodeUrl;
            var node = new Uri(uriString);
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex(DefaultIndex);
            var userName = UserName;
            if (!string.IsNullOrEmpty(userName))
                settings.
                    BasicAuthentication(userName, Password);
#if DEBUG
            settings.EnableDebugMode();
#endif
            var client = new ElasticClient(settings);
            return _elasticClient = client;
        }
        private ElasticClient GetDb(string index)
        {
            if (TypeTable.Count == 0)
                throw new Exception("Elastic Search Mapping Is Not Defined Call The Init Method First");
            this.DefaultIndex = $"{index.ToLower()}";
            var uriString = NodeUrl;
            var node = new Uri(uriString);
            var settings = new ConnectionSettings(node);
            settings.DefaultIndex(DefaultIndex);
            var userName = UserName;
            if (!string.IsNullOrEmpty(userName))
                settings.
                    BasicAuthentication(userName, Password);
#if DEBUG
            settings.EnableDebugMode();
#endif
            var client = new ElasticClient(settings);
            return _elasticClient = client;
        }

        public async Task<ICreateIndexResponse> CreateMapping<T>(Func<TypeMappingDescriptor<T>, ITypeMapping> selector) where T : class
        {
            GetDb<T>();
            return await _elasticClient.CreateIndexAsync(this.DefaultIndex, c => c
                .Mappings(ms => ms
                    .Map<T>(selector
                    )
                )
            );
        }

        public async Task<ISearchResponse<T>> Search<T>(Func<SearchDescriptor<T>, ISearchRequest> func) where T : class
        {
            GetDb<T>();
            var res = await _elasticClient.SearchAsync<T>(func);
#if DEBUG
            var rawQuery = Encoding.UTF8.GetString(res.ApiCall.RequestBodyInBytes);
            var rawres = Encoding.UTF8.GetString(res.ApiCall.ResponseBodyInBytes);
#endif
            return res;
        }


        public async Task<ISearchResponse<T>> Scroll<T>(Time time, string scrolId) where T : class
        {
            GetDb<T>();
            var res = await _elasticClient.ScrollAsync<T>(time, scrolId);
#if DEBUG
            var rawQuery = Encoding.UTF8.GetString(res.ApiCall.RequestBodyInBytes);
            var rawres = Encoding.UTF8.GetString(res.ApiCall.ResponseBodyInBytes);
#endif
            return res;
        }



        public async Task<ISearchResponse<T>> SearchAndSort<T>(int size, Func<SortDescriptor<T>, IPromise<IList<ISort>>> sort, int @from = 0) where T : class
        {
            GetDb<T>();
            return await _elasticClient.SearchAsync<T>(s => s
                .From(from)
                .Size(size)
                .Sort(sort)
            );
        }
        public async Task<ISearchResponse<T>> SearchAndQuery<T>(int size = 10000, int from = 0, Expression<Func<T, object>> field = null, object fieldValue = null) where T : class
        {
            GetDb<T>();
            if (field != null)
            {
                // throw new Exception("field is not working yet message by ramin!");
                return await _elasticClient.SearchAsync<T>(s => s
                    .From(from)
                    .Size(size)
                    .Query(q => q.MatchPhrase(a => a.Field(field).Query(fieldValue.ToString())))
                );
            }
            return await _elasticClient.SearchAsync<T>(s => s
                    .From(from)
                    .Size(size));
        }

        public async Task<ISearchResponse<T>> SearchAndQueryAndSort<T>(int size, Expression<Func<T, object>> field,
            object fieldValue, Func<SortDescriptor<T>, IPromise<IList<ISort>>> sort, int? @from = null) where T : class
        {
            GetDb<T>();
            if (from != null)
                return await _elasticClient.SearchAsync<T>(s => s
                    .From((int)@from)
                    .Size(size)
                    .Query(q => q.MatchPhrase(a => a.Field(field).Query(fieldValue.ToString())))
                    .Sort(sort)
                );
            return await _elasticClient.SearchAsync<T>(s => s
                .Size(size)
                .Query(q => q.MatchPhrase(a => a.Field(field).Query(fieldValue.ToString())))
                .Sort(sort)
            );
        }

        public async Task<IGetResponse<T>> Get<T>(T document, Id key) where T : class
        {
            GetDb<T>();
            return await _elasticClient.GetAsync<T>(document, id => new GetRequest(DefaultIndex, TypeName.Create(typeof(T)), key));
        }

        public async Task<IGetResponse<T>> GetAsync<T>(T document, Id key) where T : class
        {
            GetDb<T>();
            return await _elasticClient.GetAsync<T>(document, id => new GetRequest(DefaultIndex, TypeName.Create(typeof(T)), key));
        }

        public async Task<IIndexResponse> AddOrUpdate<T>(T document, Id key) where T : class
        {
            GetDb<T>();
            return await _elasticClient.IndexAsync<T>(document, a => a.Id(key));
        }
        public async Task<IBulkResponse> AddOrUpdateMany<T>(IEnumerable<T> documents, bool ignoreIndex = false) where T : class
        {
            if (!ignoreIndex)
                GetDb<T>();
            return await _elasticClient.IndexManyAsync<T>(documents, IndexName.Rebuild(this.DefaultIndex, typeof(T)));
        }

        public async Task DeleteAsync<T>(Id key) where T : class
        {
            GetDb<T>();
            await _elasticClient.DeleteAsync<T>(DocumentPath<T>.Id(key));
        }

        public void Dispose()
        {
            try
            {
                this._elasticClient.ConnectionSettings.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
