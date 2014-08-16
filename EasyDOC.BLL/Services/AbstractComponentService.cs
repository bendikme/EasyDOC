using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.DAL.Repositories;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class AbstractComponentService<T> : GenericService<T>, IComponentService<T> where T : AbstractComponent
    {
        public AbstractComponentService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork, IRepository<T> repository)
            : base(validationDictionary, unitOfWork, repository) { }

        public void AddManual(AbstractComponent component, int manualId, string name = "", bool includeInManual = true, bool includedInPrintedVersion = false, int? vendorId = null, FileRole role = FileRole.Other)
        {
            var manual = UnitOfWork.FileRepository.GetByKey(manualId);
            component.Files.Add(new ComponentFile
            {
                File = manual,
				Component = component,
				IncludeInManual = includeInManual,
				IncludedPrintedVersion = includedInPrintedVersion,
				VendorId = vendorId,
				Name = name,
				Role = role
            });
        }

        public void RemoveManual(AbstractComponent component, int manualId)
        {
            var child = component.Files.Single(c => c.FileId == manualId);
            component.Files.Remove(child);
        }

        public void AddMaintenance(AbstractComponent component, int maintenanceId, string remarks, bool includeInManual)
        {
            var maintenance = UnitOfWork.MaintenanceRepository.GetByKey(maintenanceId);
            component.Maintenances.Add(new ComponentMaintenance
            {
                Maintenance = maintenance,
                Component = component,
                Remarks = remarks,
                IncludeInManual = includeInManual
            });
        }

        public void RemoveMaintenance(AbstractComponent component, int maintenanceId)
        {
            var child = component.Maintenances.Single(c => c.MaintenanceId == maintenanceId);
            component.Maintenances.Remove(child);
        }

        public void AddSafety(AbstractComponent component, int safetyId, bool includeInManual)
        {
            var safety = UnitOfWork.SafetyRepository.GetByKey(safetyId);
            component.Safeties.Add(new ComponentSafety
            {
                Safety = safety,
                Component = component,
                IncludeInManual = includeInManual
            });
        }

        public void RemoveSafety(AbstractComponent component, int safetyId)
        {
           var safety = component.Safeties.Single(c => c.SafetyId == safetyId);
           component.Safeties.Remove(safety);
        }
    }
}