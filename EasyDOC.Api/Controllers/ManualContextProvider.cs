using System;
using System.Collections.Generic;
using System.Linq;
using Breeze.ContextProvider;
using Breeze.ContextProvider.EF6;
using EasyDOC.BLL.Services;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using EasyDOC.Models;
using WebMatrix.WebData;

namespace EasyDOC.Api.Controllers
{
    public class ManualContextProvider : EFContextProvider<ManualContext>
    {
        private readonly IEnumerable<RolePermission> _permissions;

        private readonly Dictionary<Type, IEntitySaveStrategy> _strategiesAfter = new Dictionary<Type, IEntitySaveStrategy>
        {
            {typeof (User), new UserStrategyAfter()}
        };

        private readonly Dictionary<Type, IEntitySaveStrategy> _strategiesBefore = new Dictionary<Type, IEntitySaveStrategy>
        {
            {typeof (ProjectFile), new ProjectFileStrategy()},
            {typeof (File), new FileStrategy()},
            {typeof (User), new UserStrategyBefore()},
            {typeof (Folder), new FolderStrategy()}
        };

        private readonly User _user;

        public ManualContextProvider()
        {
            var uow = new UnitOfWork(EntityConnection);
            _user = uow.UserRepository.GetByKeyNoTracking(WebSecurity.CurrentUserId);
            _permissions = _user.Roles.SelectMany(r => r.Role.Permissions);
        }

        protected override Dictionary<Type, List<EntityInfo>> BeforeSaveEntities(Dictionary<Type, List<EntityInfo>> saveMap)
        {
            var errors = new List<EFEntityError>();

            foreach (var pair in saveMap)
            {
                List<EntityInfo> entityList = pair.Value;

                // Sort the entityinfo-list so that deleted items are processed first
                foreach (EntityInfo entityInfo in entityList.OrderBy(e => e.EntityState))
                {
                    Type type = entityInfo.Entity.GetType();

                    foreach (string property in new[] { "EditorId", "Edited", "CreatorId", "Created" })
                    {
                        if (entityInfo.OriginalValuesMap.ContainsKey(property))
                            errors.Add(new EFEntityError(entityInfo, "Unmutable field", "Cannot change unmutable field of entity", property));
                    }

                    if (errors.Any())
                        throw new EntityErrorsException(errors);


                    if (!CheckPermissions(type, entityInfo.Entity, entityInfo.EntityState))
                        throw new Exception("The following permission is needed in order to complete the requested operation: " + type.Name + " - " + entityInfo.EntityState);

                    if (entityInfo.EntityState == EntityState.Deleted)
                    {
                        var entity = entityInfo.Entity as DatabaseObject;
                        if (entity != null)
                            if (!entity.CanDelete())
                                throw new Exception("Cannot delete the item because other items depend on it. Remove all references to this item in order to delete it.");
                    }

                    if (entityInfo.Entity is IHistoryEntity)
                    {
                        var entity = entityInfo.Entity as IHistoryEntity;


                        if (entityInfo.EntityState == EntityState.Added)
                        {
                            entity.Created = DateTime.UtcNow;
                            entity.Edited = DateTime.UtcNow;
                            entity.CreatorId = WebSecurity.CurrentUserId;
                            entityInfo.OriginalValuesMap.Add("CreatorId", null);
                        }
                        else if (entityInfo.EntityState == EntityState.Modified)
                        {
                            entity.Edited = DateTime.UtcNow;
                            entity.EditorId = WebSecurity.CurrentUserId;
                            entityInfo.OriginalValuesMap.Add("EditorId", null);
                        }
                    }

                    Type entityType = pair.Key;

                    if (_strategiesBefore.ContainsKey(entityType))
                        _strategiesBefore[entityType].Execute(EntityConnection, entityInfo);
                }
            }

            return saveMap;
        }

        protected override void AfterSaveEntities(Dictionary<Type, List<EntityInfo>> saveMap, List<KeyMapping> keyMappings)
        {
            var uow = new UnitOfWork(EntityConnection);
            var service = new GenericService<DatabaseLog>(null, uow, uow.DatabaseLogRepository);

            foreach (var pair in saveMap)
            {
                var entityList = pair.Value;

                foreach (var entityInfo in entityList)
                {
                    var type = entityInfo.Entity.GetType();

                    foreach (var info in pair.Value)
                    {
                        if (_strategiesAfter.ContainsKey(pair.Key))
                        {
                            _strategiesAfter[pair.Key].Execute(EntityConnection, info);
                        }

                        var id = entityInfo.Entity as IIdentifyable;
                        if (id != null)
                        {
                            var content = string.Format("Type:{0}\nKeys:{1}\nState:{2}", type.Name, id.GetId(), info.EntityState);

                            foreach (var property in info.OriginalValuesMap.Keys)
                            {
                                var originalValue = info.OriginalValuesMap[property];

                                if (originalValue != null)
                                {
                                    content += string.Format("\n{0}:{1}", property, originalValue);
                                }
                            }

                            service.Create(new DatabaseLog
                            {
                                Name = "Log entry",
                                Content = content,
                                CreatorId = WebSecurity.CurrentUserId
                            });
                        }
                    }
                }
            }

            uow.Commit();
            uow.DetachAll();
        }

        private bool CheckPermissions(Type type, object entity, EntityState entityState)
        {
            RolePermission permission = _permissions.FirstOrDefault(p => p.Permission.Name == type.Name);

            if (permission == null)
                return false;

            var historyEntity = entity as IHistoryEntity;
            int? ownerId = -1;
            if (historyEntity != null)
                ownerId = historyEntity.CreatorId;

            switch (entityState)
            {
                case EntityState.Added:
                    return permission.Create == RoleScope.All;

                case EntityState.Deleted:
                    return CheckRights(ownerId, permission.Delete);

                case EntityState.Modified:
                    return CheckRights(ownerId, permission.Update);

                default:
                    return false;
            }
        }

        private bool CheckRights(int? ownerId, RoleScope scope)
        {
            if (scope == RoleScope.All)
                return true;

            if (ownerId != null)
                return scope == RoleScope.Owned && ownerId == _user.Id;

            return false;
        }
    }
}