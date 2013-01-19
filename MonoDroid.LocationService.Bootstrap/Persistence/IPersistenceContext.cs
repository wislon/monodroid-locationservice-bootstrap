using System.Collections.Generic;

namespace MonoDroid.LocationService.Bootstrap.Persistence
{
    public interface IPersistenceContext<T>
    {
        IEnumerable<T> List();
        T GetById(string id);
        void Insert(T data);
        void SaveChanges();
        void CancelChanges();
        string ExportData();
    }
}