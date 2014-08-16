using System.Collections.Generic;
using EasyDOC.Model;

namespace EasyDOC.DAL.Repositories
{
    public interface ISoftDeletableRepository<T> : IRepository<T> where T : DatabaseObject, ISoftDeletable {
        IEnumerable<T> GetAllDeleted();
        void Restore(int id);
    }
}