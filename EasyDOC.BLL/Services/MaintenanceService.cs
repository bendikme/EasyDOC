using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class MaintenanceService : GenericService<Maintenance>, IMaintenanceService
    {
        public MaintenanceService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork)
            : base(validationDictionary, unitOfWork, unitOfWork.MaintenanceRepository) { }

    }
}