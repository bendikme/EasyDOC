using System;
using System.Data.Common;
using System.IO;
using System.Web;
using Breeze.ContextProvider;
using EasyDOC.BLL.Services;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Api.Controllers
{
    internal class FolderStrategy : IEntitySaveStrategy
    {
        public bool Execute(DbConnection connection, EntityInfo info)
        {
            var root = HttpContext.Current.Server.MapPath("~/");
            var uow = new UnitOfWork(connection);

            var folderService = new CatalogService(null, uow);

            var newFolder = info.Entity as Folder;
            var folder = folderService.GetByKey(newFolder.Id);

            switch (info.EntityState)
            {
                case EntityState.Added:

                    if (newFolder.ParentId.HasValue)
                    {
                        var parentFolder = folderService.GetByKey(newFolder.ParentId.Value);
                        var newPath = root + parentFolder.GetPath() + '\\' + newFolder.Name;

                        if (Directory.Exists(newPath))
                        {
                            throw new Exception("A folder named " + newFolder.Name + " already exists.");
                        }

                        Directory.CreateDirectory(newPath);
                    }
                    break;

                case EntityState.Deleted:
                    if (folder.CanDelete())
                    {
                        var path = root + folder.GetPath();
                        Directory.Delete(path);
                    }
                    else
                    {
                        throw new Exception("Cannot delete the folder because it is not empty");
                    }
                    break;

                case EntityState.Modified:

                    // Move the file to its new parent folder
                    if (info.OriginalValuesMap.ContainsKey("ParentId") || info.OriginalValuesMap.ContainsKey("Name"))
                    {
                        if (newFolder.ParentId.HasValue)
                        {
                            var parentFolder = folderService.GetByKey(newFolder.ParentId.Value);
                            var oldPath = root + folder.GetPath();
                            var newPath = root + parentFolder.GetPath() + '\\' + newFolder.Name;

                            Directory.Move(oldPath, newPath);
                        }
                        else
                        {
                            throw new Exception("Folder must have parent");
                        }
                    }
                    break;
            }

            uow.DetachAll();
            return true;
        }
    }
}