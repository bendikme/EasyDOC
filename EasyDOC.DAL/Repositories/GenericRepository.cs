using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using EasyDOC.Model;

namespace EasyDOC.DAL.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : DatabaseObject
    {
        protected readonly DbContext Context;
        protected readonly DbSet<T> DbSet;

        public GenericRepository(DbContext context)
        {
            Context = context;
            DbSet = context.Set<T>();
        }

        public virtual T GetByKeyNoTracking(int id)
        {
            return DbSet.AsNoTracking().SingleOrDefault(m => m.Id == id);
        }

        public virtual IEnumerable<T> GetAll()
        {
            return DbSet.ToList();
        }

		public virtual IEnumerable<T> GetAllNoTracking()
		{
		    return DbSet.AsNoTracking().ToList();
		}

        public virtual T GetByKey(int id)
        {
            return DbSet.Find(id);
        }

        public virtual IEnumerable<T> Get(
            Expression<Func<T, bool>> filter = null, 
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, 
            string includes = "", bool noTracking = false)
        {
            IQueryable<T> query = DbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            query = includes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Aggregate(query, (current, property) => current.Include(property));

            return orderBy == null ? query.ToList() : orderBy(query).ToList();
        }

        public virtual void Create(T item)
        {
            DbSet.Add(item);
        }


        public virtual void Update(T item)
        {
            DbSet.Attach(item);
            Context.Entry(item).State = EntityState.Modified;
        }


        public virtual void Delete(int id)
        {
            var entity = DbSet.Find(id);
            DbSet.Remove(entity);
        }

        public IQueryable<T> GetQueryable()
        {
            return DbSet;
        }

        public T Refresh(T item)
        {
            return DbSet.AsNoTracking().FirstOrDefault(i => i.Id == item.Id);
        }

        public virtual void Delete(T item)
        {
            if (Context.Entry(item).State == EntityState.Detached)
            {
                DbSet.Attach(item);
            }

            DbSet.Remove(item);
        }
    }
}