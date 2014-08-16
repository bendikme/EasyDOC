using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace EasyDOC.DAL.Repositories
{
    public interface IRepository<T>
    {
        T GetByKey(int id);
        T GetByKeyNoTracking(int id);

        IEnumerable<T> GetAll();
        IEnumerable<T> GetAllNoTracking();

        IEnumerable<T> Get(
            Expression<Func<T, bool>> filter,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includes = "", bool noTracking = false
        );

        void Create(T item);
        void Update(T item);
        void Delete(T item);
        void Delete(int id);
        IQueryable<T> GetQueryable();
        T Refresh(T item);
    }
}
