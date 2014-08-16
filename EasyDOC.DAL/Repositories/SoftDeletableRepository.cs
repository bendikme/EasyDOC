using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.DAL.Repositories
{
    public class SoftDeletableRepository<T> : GenericRepository<T>, ISoftDeletableRepository<T> where T : DatabaseObject, ISoftDeletable
    {
        public SoftDeletableRepository(DbContext context) : base(context) {}

        public override T GetByKeyNoTracking(int id)
        {
            return DbSet.AsNoTracking().SingleOrDefault(m => m.Id == id && !m.Deleted.HasValue);
        }

        public override IEnumerable<T> GetAll()
        {
            return DbSet.ToList().Where(m => !m.Deleted.HasValue);
        }

		public override IEnumerable<T> GetAllNoTracking()
		{
		    return DbSet.AsNoTracking().ToList().Where(m => !m.Deleted.HasValue);
		}

		public IEnumerable<T> GetAllDeleted()
		{
		    return DbSet.ToList().Where(m => m.Deleted.HasValue);
		}

        public void Restore(int id)
        {
            GetByKey(id).Deleted = null;
        }

        public override T GetByKey(int id)
        {
            return DbSet.SingleOrDefault(m => m.Id == id && !m.Deleted.HasValue);
        }

        public override IEnumerable<T> Get(
            Expression<Func<T, bool>> filter = null, 
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, 
            string includes = "", bool noTracking = false)
        {
            return base.Get(filter, orderBy, includes).Where(m => !m.Deleted.HasValue);
        }

        public override void Delete(int id)
        {
            var item = DbSet.Find(id);

			if (!item.Deleted.HasValue)
			{
			    item.Deleted = DateTime.Now;
			}
			else
			{
			    item.RemoveAllReferences();
				DbSet.Remove(item);
			}
        }

        public override void Delete(T item)
        {
            if (Context.Entry(item).State == EntityState.Detached)
            {
                DbSet.Attach(item);
            }

			if (!item.Deleted.HasValue)
			{
			    item.Deleted = DateTime.Now;
			}
			else
			{
				DbSet.Remove(item);
			}
        }
    }
}