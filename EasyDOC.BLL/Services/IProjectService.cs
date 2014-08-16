using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public interface IProjectService : IGenericService<Project>
    {
        void AddFile(Project project, int fileId, string name, bool includeInManual, bool includedInPrintedVersion, int? vendorId, FileRole role);
        void RemoveFile(Project project, int fileId);

        void AddComponent(Project project, int componentId, int count, string info, bool includeInManual = true);
        void RemoveComponent(Project project, int componentId);

        void AddMaintenance(Project project, int maintenanceId, string remarks, bool includeInManual);
        void RemoveMaintenance(Project project, int maintenanceId);

        void AddSafety(Project project, int safetyId, bool includeInManual);
        void RemoveSafety(Project project, int safetyId);
    }
}