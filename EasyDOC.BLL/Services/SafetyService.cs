using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class SafetyService : GenericService<Safety>, ISafetyService
    {
        public SafetyService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork)
            : base(validationDictionary, unitOfWork, unitOfWork.SafetyRepository) { }

        public void AddComponent(Safety safety, int componentId, bool includeInManual = true)
        {
            var component = UnitOfWork.ComponentRepository.GetByKey(componentId);
            safety.Components.Add(new ComponentSafety
            {
                Component = component,
                Safety = safety,
				IncludeInManual = includeInManual
            });
        }

        public void RemoveComponent(Safety safety, int componentId)
        {
            var child = safety.Components.Single(c => c.ComponentId == componentId);
            safety.Components.Remove(child);
        }

        public void AddProject(Safety safety, int projectId)
        {
            var project = UnitOfWork.ProjectRepository.GetByKey(projectId);
            safety.Projects.Add(new ProjectSafety
            {
                Project = project,
                Safety = safety
            });
        }

        public void RemoveProject(Safety safety, int projectId)
        {
            var child = safety.Projects.Single(c => c.ProjectId == projectId);
            safety.Projects.Remove(child);
        }
    }
}