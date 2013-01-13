using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Android.Content;

namespace MonoDroid.LocationService.Bootstrap.Persistence
{
    /// <summary>
    /// Repository for storing textual data.
    /// Default constructor requires an application context to be passed in, so that the persistence context
    /// it loads (and passes it on to) can access the file system methods.
    /// </summary>
    /// <typeparam name="T">The type of data to be persisted</typeparam>
    /// TODO Get it to work out what kind of persistence store it needs
    public class Repository<T> : IRepository<T> where T: class 
    {
        private readonly Context _context;
        private readonly IPersistenceContext<T> _persistenceContext;

        public Repository(Context context)
        {
            _context = context;
            _persistenceContext = new TextFilePersistenceContext<T>(_context); // TODO need a service locator or factory
        }

        public void Insert(T data)
        {
            _persistenceContext.Insert(data);
        }


        public IEnumerable<T> List()
        {
            return _persistenceContext.List();
        }

        public T GetById(string id)
        {
            return _persistenceContext.GetById(id);
        }

        public void SaveChanges()
        {
            _persistenceContext.SaveChanges();
        }

        public IQueryable<T> FindBy(Expression<Func<T, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public void Update(T data)
        {
            throw new NotImplementedException();
        }
    }
}