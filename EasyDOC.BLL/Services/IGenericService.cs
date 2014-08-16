using System.Collections.Generic;

namespace EasyDOC.BLL.Services
{
    public interface IGenericService<T>
    {
        T GetByKey(int id);
        T GetByKeyNoTracking(int id);
        IEnumerable<T> GetAll();

        void Create(T item);
        void Update(T item);
        void Delete(int id);

        void Commit();
        void Dispose();
        T Refresh(T item);
    }
}