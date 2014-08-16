using System;
using System.Collections.Generic;
using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.DAL.Repositories;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class GenericService<T> : IGenericService<T> where T : DatabaseObject
    {
        public IValidationDictionary ValidationDictionary { get; private set; }

        protected readonly IUnitOfWork UnitOfWork;
        protected readonly IRepository<T> Repository;

        public GenericService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork, IRepository<T> repository)
        {
            ValidationDictionary = validationDictionary;
            UnitOfWork = unitOfWork;
            Repository = repository;
        }

        public virtual T GetByKey(int id)
        {
            return Repository.GetByKey(id);
        }

        public T GetByKeyNoTracking(int id)
        {
            return Repository.GetByKeyNoTracking(id);
        }

        public virtual IEnumerable<T> GetAll()
        {
            return Repository.GetAll();
        }

        public virtual void Create(T item)
        {
            Repository.Create(item);
        }

        public virtual void Update(T item)
        {
            Repository.Update(item);
        }

        public virtual void Delete(int id)
        {
            if (ValidationDictionary.IsValid)
            {
                var model = Repository.GetByKey(id);
				model.RemoveAllReferences();
                Repository.Delete(id);
            }
        }

        protected void AddRelation<TParent, TChild>(ICollection<TChild> collection, TParent parent, TChild child)
            where TParent : IEntity
            where TChild : IEntity
        {
            if (collection.Any(i => i.Name == child.Name))
                throw new ArgumentException("Not unique name");

            collection.Add(child);
        }

        protected void AddOrderedRelation<TRelation, TParent, TChild>(ICollection<TRelation> collection, TParent parent, TChild child)
            where TParent : IEntity
            where TChild : IEntity
            where TRelation : IOrderedRelation<TParent, TChild>
        {
            if (collection.Any(i => i.Child.Name == child.Name))
                throw new ArgumentException("Not unique name");

            var relation = Activator.CreateInstance<TRelation>();
            relation.Parent = parent;
            relation.Child = child;
            relation.Order = collection.MaxOrDefault(r => r.Order) + 1;

            collection.Add(relation);
        }

        public virtual void Commit()
        {
            if (ValidationDictionary == null || ValidationDictionary.IsValid)
            {
                UnitOfWork.Commit();
            }
        }

        public void Dispose()
        {
            UnitOfWork.Dispose();
        }

        public T Refresh(T item)
        {
            return Repository.Refresh(item);
        }
    }
}
