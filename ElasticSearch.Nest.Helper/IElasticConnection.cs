using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nest;

namespace ES.Helper
{
    public interface IElasticConnection
    {
        void InitDb(Dictionary<Type, string> typeTables, string dbName);
        void SetDefaultConnection(string nodeUrl, string userName = null, string password = null);
        void SetToProductionEnv();
        void SetToTestEnv();
        ElasticConnection GetLastDefaultEsConnection();
        string CreateQuery(string operatorWord, List<string> words);
        Task CopyFromTo<TS>(string sourceIndex, string destinationIndex) where TS : class;
        Task<ICreateIndexResponse> CreateMapping<T>(Func<TypeMappingDescriptor<T>, ITypeMapping> selector) where T : class;
        Task<ISearchResponse<T>> Search<T>(Func<SearchDescriptor<T>, ISearchRequest> func) where T : class;
        Task<ISearchResponse<T>> SearchAndSort<T>(int size, Func<SortDescriptor<T>, IPromise<IList<ISort>>> sort, int @from = 0) where T : class;
        Task<ISearchResponse<T>> SearchAndQuery<T>(int size = 10000, int from = 0, Expression<Func<T, object>> field = null, object fieldValue = null) where T : class;
        Task<ISearchResponse<T>> SearchAndQueryAndSort<T>(int size, Expression<Func<T, object>> field,
            object fieldValue, Func<SortDescriptor<T>, IPromise<IList<ISort>>> sort, int? @from = null) where T : class;
        Task<ISearchResponse<T>> Scroll<T>(Time time, string scrolId) where T : class;
        Task<IGetResponse<T>> GetAsync<T>(Id key) where T : class;
        Task<IIndexResponse> AddOrUpdate<T>(T document, Id key) where T : class;
        Task<IBulkResponse> AddOrUpdateMany<T>(IEnumerable<T> documents, bool ignoreIndex = false) where T : class;
        Task<IDeleteResponse> DeleteAsync<T>(Id key) where T : class;
    }
}