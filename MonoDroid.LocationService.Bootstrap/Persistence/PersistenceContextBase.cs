using System;
using System.Collections.Generic;
using Android.Content;
using Android.Runtime;

namespace MonoDroid.LocationService.Bootstrap.Persistence
{
    /// <summary>
    /// Inherits from ContextWrapper so that derived classes can get at the file system methods,
    /// as well as other stuff
    /// </summary>
    /// <typeparam name="T">Generic parameter indicating the type of data object being stored</typeparam>
    public abstract class PersistenceContextBase<T> : ContextWrapper, IPersistenceContext<T>
    {

        protected PersistenceContextBase(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected PersistenceContextBase(Context context) : base(context)
        {
        }


        /// <summary>
        /// Unit of Work pattern - Insert the result into the queue of operations, 
        /// before SaveChanges(), so you can batch/cancel/wrap a bunch of operations 
        /// in a transaction, etc.
        /// </summary>
        /// <param name="data"></param>
        public abstract void Insert(T data);

        /// <summary>
        /// Unit of Work - Maybe you need to throw away all the  work the user has done 
        /// and reset everything? 
        /// </summary>
        public abstract void CancelChanges();

        /// <summary>
        /// Loads all items
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<T> List();

        /// <summary>
        /// Loads a specific Item. Overload if you need to load it 
        /// by a different type
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public abstract T GetById(string id);

        /// <summary>
        /// Unit of Work - Actually persists the changes permanently.
        /// </summary>
        /// <returns></returns>
        public abstract void SaveChanges();
    }
}