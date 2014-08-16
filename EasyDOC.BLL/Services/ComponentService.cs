using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class ComponentService : AbstractComponentService<Component>
    {
        public ComponentService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork)
            : base(validationDictionary, unitOfWork, unitOfWork.SingleComponentRepository) { }

        public override void Update(Component component)
        {
            if (Repository.GetAllNoTracking().Any(t => t.Name == component.Name && t.VendorId == component.VendorId && t.Id != component.Id))
            {
                ValidationDictionary.AddError("ArticleNumber", "Unique component required");
            }
            else
            {
                base.Update(component);
            }
        }
    }
}