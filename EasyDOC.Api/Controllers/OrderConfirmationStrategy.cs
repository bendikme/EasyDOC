using System.Data.Common;
using System.Linq;
using System.Web;
using Breeze.WebApi;
using EasyDOC.BLL.Services;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Api.Controllers
{
    internal class OrderConfirmatioStrategy : IEntitySaveStrategy
    {

        public bool Execute(DbConnection connection, EntityInfo info)
        {
            var root = HttpContext.Current.Server.MapPath("~/");
            var uow = new UnitOfWork(connection);
            var service = new ProjectService(null, uow, root);

            var relation = info.Entity as ProjectFile;
            var project = service.GetByKey(relation.ProjectId);

            if (info.EntityState == EntityState.Modified || info.EntityState == EntityState.Deleted)
            {
                foreach (var comp in project.Components.Where(c => c.FileId == relation.FileId).ToList())
                {
                    project.Components.Remove(comp);
                }
            }

            if (info.EntityState == EntityState.Added || info.EntityState == EntityState.Modified)
            {
                var file = uow.FileRepository.GetByKey(relation.FileId);
                var vendor = service.ParseAndAddComponents(project, relation.Role, file);

                if (!info.OriginalValuesMap.ContainsKey("VendorId"))
                {
                    relation.VendorId = vendor == null ? null : (int?)vendor.Id;
                    info.OriginalValuesMap.Add("VendorId", null);
                }
            }

            service.Commit();
            uow.DetachAll();

            return true;
        }
    }
}