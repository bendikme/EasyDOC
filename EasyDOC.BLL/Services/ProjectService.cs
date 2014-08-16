using System;
using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;
using EasyDOC.Pdf.Interface;

namespace EasyDOC.BLL.Services
{
    public class ProjectService : GenericService<Project>, IProjectService
    {
        private readonly string _rootDirectory;

        public ProjectService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork, string rootDirectory)
            : base(validationDictionary, unitOfWork, unitOfWork.ProjectRepository)
        {
            _rootDirectory = rootDirectory;
        }

        public override void Update(Project project)
        {
            if (Repository.Get(t => t.ProjectNumber == project.ProjectNumber && t.Id != project.Id).Any())
            {
                ValidationDictionary.AddError("ProjectNumber", "Unique project number required");
            }
            else
            {
                base.Update(project);
            }
        }

        public void AddFile(Project project, int fileId, string name = "", bool includeInManual = true, bool includedInPrintedVersion = false, int? vendorId = null, FileRole role = FileRole.Other)
        {
            var file = UnitOfWork.FileRepository.GetByKey(fileId);

            var vendor = ParseAndAddComponents(project, role, file);

            project.Files.Add(new ProjectFile
            {
                File = file,
                Project = project,
                IncludeInManual = includeInManual,
                IncludedPrintedVersion = includedInPrintedVersion,
                VendorId = vendor == null ? vendorId : vendor.Id,
                Name = name,
                Role = role
            });
        }

        public Vendor ParseAndAddComponents(Project project, FileRole role, File file)
        {
            /*var parser = OrderConfirmationFactory.GetParser(_rootDirectory, role);

            if (parser != null)
            {
                var service = new ComponentService(ValidationDictionary, UnitOfWork);
                var vendor = UnitOfWork.VendorRepository
                    .Get(v => v.Name == parser.VendorName)
                    .SingleOrDefault();

                if (vendor == null)
                {
                    var vendorService = new VendorService(ValidationDictionary, UnitOfWork);
                    vendor = new Vendor { Name = parser.VendorName };
                    vendorService.Create(vendor);
                    vendorService.Commit();
                }

                foreach (var item in parser.ExtractTabularDataFromPdf())
                {
                    if (string.IsNullOrWhiteSpace(item.Article))
                    {
                        item.Article = string.Format("unique-id[{0}]", item.Description.GetHashCode());
                    }

                    var item1 = item;
                    var component = UnitOfWork.SingleComponentRepository
                        .Get(c => c.Name == item1.Article && c.VendorId == vendor.Id)
                        .SingleOrDefault();

                    if (component == null)
                    {
                        component = new Component
                        {
                            Description = item.Description,
                            Name = item.Article,
                            Vendor = vendor
                        };

                        service.Create(component);
                        service.Commit();
                    }

                    if (!project.Components.Any(c => c.File == file && c.Component.Name == component.Name && c.Component.Vendor == vendor))
                    {
                        AddComponent(project, component.Id, (Decimal)item.Count, "", true, file);
                    }
                }
                return vendor;
            }*/

            return null;
        }

        public void RemoveFile(Project project, int fileId)
        {
            var child = project.Files.Single(c => c.FileId == fileId);
            project.Files.Remove(child);

            foreach (var comp in project.Components.Where(c => c.FileId == child.FileId).ToList())
            {
                project.Components.Remove(comp);
            }
        }

        public void AddComponent(Project project, int componentId, int count, string info, bool includeInManual = true)
        {
            AddComponent(project, componentId, count, info, includeInManual, null);
        }

        public void AddComponent(Project project, int componentId, Decimal count, string info, bool includeInManual, File file)
        {
            var component = UnitOfWork.ComponentRepository.GetByKey(componentId);
            var relation = project.Components.SingleOrDefault(pc => pc.ComponentId == componentId);

            if (relation != null)
            {
                relation.Count += count;
                relation.File = file;
            }
            else
            {
                project.Components.Add(new ProjectComponent
                {
                    Component = component,
                    Project = project,
                    Count = count,
                    Info = info,
                    File = file,
                    IncludeInManual = includeInManual
                });
            }
        }

        public void RemoveComponent(Project project, int componentId)
        {
            var child = project.Components.Single(c => c.ComponentId == componentId);
            project.Components.Remove(child);
        }

        public void AddMaintenance(Project project, int maintenanceId, string remarks, bool includeInManual)
        {
            var maintenance = UnitOfWork.MaintenanceRepository.GetByKey(maintenanceId);
            project.Maintenances.Add(new ProjectMaintenance
            {
                Maintenance = maintenance,
                Project = project,
                Remarks = remarks,
                IncludeInManual = includeInManual
            });
        }

        public void RemoveMaintenance(Project project, int maintenanceId)
        {
            var child = project.Maintenances.Single(c => c.MaintenanceId == maintenanceId);
            project.Maintenances.Remove(child);
        }

        public void AddSafety(Project project, int safetyId, bool includeInManual)
        {
            var safety = UnitOfWork.SafetyRepository.GetByKey(safetyId);
            project.Safeties.Add(new ProjectSafety
            {
                Safety = safety,
                Project = project,
                IncludeInManual = includeInManual
            });
        }

        public void RemoveSafety(Project project, int safetyId)
        {
            var child = project.Safeties.Single(c => c.SafetyId == safetyId);
            project.Safeties.Remove(child);
        }
    }
}