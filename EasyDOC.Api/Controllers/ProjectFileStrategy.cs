using System;
using System.Data.Common;
using System.Linq;
using System.Web;
using Breeze.ContextProvider;
using EasyDOC.BLL.Services;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.Api.Controllers
{
    internal class ProjectFileStrategy : IEntitySaveStrategy
    {

        public bool Execute(DbConnection connection, EntityInfo info)
        {
            var root = HttpContext.Current.Server.MapPath("~/");
            var uow = new UnitOfWork(connection);

            var projectService = new ProjectService(null, uow, root);
            var orderService = new OrderConfirmationService(null, uow, root);

            var relation = info.Entity as ProjectFile;
            var project = projectService.GetByKey(relation.ProjectId);

            if (info.EntityState == EntityState.Added && relation.Role == FileRole.OrderConfirmation)
            {
                if (uow.OrderConfirmationRepository.Get(o => o.FileId == relation.FileId).Any())
                {
                    throw new Exception("File already used as an order confirmation");
                }

                var orderConfirmation = orderService.CreateFromProjectFile(relation);
                info.OriginalValuesMap.Add("VendorId", orderConfirmation.VendorId);

                foreach (var item in orderConfirmation.Items)
                {
                    var rel = project.Components.SingleOrDefault(pc => pc.Component.Name == item.Component.Name && pc.Component.Vendor.Name == item.Component.Vendor.Name);

                    if (rel == null)
                    {
                        project.Components.Add(new ProjectComponent
                        {
                            Component = item.Component,
                            ComponentId = item.ComponentId,
                            Project = project,
                            ProjectId = project.Id,
                            Count = item.Quantity,
                            Unit = item.Unit,
                            IncludeInManual = true
                        });
                    }
                    else
                    {
                        rel.Count += item.Quantity;
                    }
                }
            }
            else if (info.EntityState == EntityState.Deleted && relation.Role == FileRole.OrderConfirmation)
            {
                var orderConfirmation = uow.OrderConfirmationRepository.Get(o => o.FileId == relation.FileId).SingleOrDefault();

                if (orderConfirmation != null)
                {
                    foreach (var item in orderConfirmation.Items)
                    {
                        var rel = project.Components.SingleOrDefault(pc => pc.ComponentId == item.ComponentId);

                        if (rel != null)
                        {
                            rel.Count -= item.Quantity;
                            if (rel.Count <= 0)
                            {
                                project.Components.Remove(rel);
                            }
                        }
                    }
                }
            }

            projectService.Commit();
            uow.DetachAll();

            return true;
        }
    }
}