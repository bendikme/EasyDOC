using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class SeriesService : AbstractComponentService<ComponentSeries>
    {
        public SeriesService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork)
            : base(validationDictionary, unitOfWork, unitOfWork.ComponentSeriesRepository) { }

        public override void Update(ComponentSeries component)
        {
            if (Repository.GetAllNoTracking().Any(t => t.Name == component.Name && t.VendorId == component.VendorId && t.Id != component.Id))
            {
                ValidationDictionary.AddError("SeriesName", "Unique name required");
            }
            else
            {
                base.Update(component);
            }
        }
    }
}