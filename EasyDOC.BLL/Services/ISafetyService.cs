using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public interface ISafetyService : IGenericService<Safety>
    {
        void AddComponent(Safety safety, int componentId, bool includeInManual = true);
        void RemoveComponent(Safety safety, int componentId);

        void AddProject(Safety safety, int projectId);
        void RemoveProject(Safety safety, int projectId);
    }
}