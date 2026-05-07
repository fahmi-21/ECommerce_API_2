namespace ECommerce_API_2.Repository
{
    public interface IRepository <T> where T : class
    {
        Task CreateAsync(T entity);
        void Edit(T entity);
        void Delete(T entity);
        Task<int> CommitAsync();
        Task<List<T>> GetAsync(Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true);
        Task<T?> GetOneAsync(Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true);
    }
}
