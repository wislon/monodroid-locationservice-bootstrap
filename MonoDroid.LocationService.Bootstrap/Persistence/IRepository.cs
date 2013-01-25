using System;
using System.Linq;
using System.Linq.Expressions;

namespace MonoDroid.LocationService.Bootstrap.Persistence
{

    public interface IRepository<T> where T: class
    {
        /// <summary>
        /// Insert an item to the Unit of Work queue, to persisted. 
        /// Call <see cref="SaveChanges"/> to actually persist the data.
        /// </summary>
        /// <param name="data"></param>
        void Insert(T data);

        /// <summary>
        /// Get a single item by its ID.
        /// </summary>
        /// <param name="id"></param>
        T GetById(string id);

        /// <summary>
        /// Find records matching whatever predicate function you feed it.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IQueryable<T> FindBy(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Physically persists the changes to the data store.
        /// </summary>
        /// <returns></returns>
        void SaveChanges();

        /// <summary>
        /// Re-persists an existing object, after it's been modified.
        /// </summary>
        /// <param name="data"></param>
        void Update(T data);

        /// <summary>
        /// Provides a method to export the data. This is done via the 
        /// persistence context (tho it's up to you to implement it).
        /// </summary>
        string ExportData();
    }


}