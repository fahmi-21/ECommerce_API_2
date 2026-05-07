using ECommerce_API_2.DataAccess;

namespace ECommerce_API_2.Repository
{
    public class Repositories <T> : IRepository<T> where T : class
    {
        protected AppDbContext _context;
        protected DbSet<T> _DbSet;

        public Repositories ( AppDbContext context)
        {
            _context = context;
            _DbSet = _context.Set<T>();
        }

        public async Task CreateAsync ( T entity)
        {
            await _DbSet.AddAsync (entity);
        }
        public void Edit ( T entity )
        {
            _DbSet.Update( entity);
        }
        public void Delete ( T entity )
        {
            _DbSet.Remove ( entity );
        }
        public async Task <int> CommitAsync ()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch ( Exception ex )
            {
                Console.WriteLine ( ex );
                return 0;
            }
        }
        public async Task<List<T>> GetAsync (Expression <Func<T , bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true )
        {
            var records = _DbSet.AsQueryable();

            if ( expression is not null )
                records = records.Where ( expression );

            if ( !tracked )
                records = records.AsNoTracking ();
            
            if ( includes is not null )
            {
                foreach ( var include in includes )
                {
                    if ( include is not null)
                        records = records.Include ( include );
                }
            }
            return  await records.ToListAsync();
        }
        public async Task<T?> GetOneAsync (Expression<Func<T, bool>>? expression = null, Expression<Func<T, object>>[]? includes = null, bool tracked = true)
        {
            return (await GetAsync(
                expression: expression,
                includes: includes,
                tracked: tracked
            )).FirstOrDefault();
        }
    }
}
