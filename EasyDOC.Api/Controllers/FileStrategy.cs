using System;
using System.Data.Common;
using System.Web;
using Breeze.ContextProvider;
using EasyDOC.BLL.Services;
using EasyDOC.DAL.DataAccess;
using File = EasyDOC.Model.File;

namespace EasyDOC.Api.Controllers
{
    internal class FileStrategy : IEntitySaveStrategy
    {

        public bool Execute(DbConnection connection, EntityInfo info)
        {
            var root = HttpContext.Current.Server.MapPath("~/");
            var uow = new UnitOfWork(connection);

            var fileService = new FileService(null, uow);
            var folderService = new CatalogService(null, uow);

            var newFile = info.Entity as File;

            if (newFile != null)
            {
                var file = fileService.GetByKey(newFile.Id);

                switch (info.EntityState)
                {
                    case EntityState.Deleted:
                        if (file.CanDelete())
                        {
                            var path = root + file.GetPath();

                            try
                            {
                                System.IO.File.Delete(path);
                            }
                            catch (Exception)
                            {
                                throw new Exception("Error deleting file " + newFile.Name);
                            }
                        }
                        break;

                    case EntityState.Modified:

                        // Move the file to its new parent folder
                        if (info.OriginalValuesMap.ContainsKey("CatalogId") || info.OriginalValuesMap.ContainsKey("Name"))
                        {
                            var newFolder = folderService.GetByKey(newFile.CatalogId);
                            var oldPath = root + file.GetPath();
                            var newPath = root + newFolder.GetPath() + '\\' + newFile.Name + '.' + file.Type;

                            try
                            {
                                System.IO.File.Move(oldPath, newPath);
                            }
                            catch (Exception e)
                            {
                                throw new Exception("Error renaming/moving file " + newFile.Name);
                            }
                        }
                        break;
                }
            }
            else
            {
                throw new Exception("Error loading entity!");
            }

            uow.DetachAll();
            return true;
        }
    }
}