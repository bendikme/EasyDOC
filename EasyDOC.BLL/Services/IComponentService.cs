using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public interface IComponentService<T> : IGenericService<T> where T : AbstractComponent
    {
        void AddManual(AbstractComponent component, int manualId, string name, bool includeInManual, bool includedInPrintedVersion, int? vendorId, FileRole role);
        void RemoveManual(AbstractComponent component, int manualId);

        void AddMaintenance(AbstractComponent project, int maintenanceId, string remarks, bool includeInManual);
        void RemoveMaintenance(AbstractComponent component, int manualId);

        void AddSafety(AbstractComponent component, int safetyId, bool includeInManual);
        void RemoveSafety(AbstractComponent component, int safetyId);
    }
}