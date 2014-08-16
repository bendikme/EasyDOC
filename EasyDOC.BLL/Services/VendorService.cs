using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class VendorService : GenericService<Vendor>
    {
        public VendorService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork)
            : base(validationDictionary, unitOfWork, unitOfWork.VendorRepository) { }

        public override void Update(Vendor vendor)
        {
            if (Repository.Get(t => t.Name == vendor.Name && t.Id != vendor.Id).Any())
            {
                ValidationDictionary.AddError("Name", "Unique name required");
            }
            else if (vendor.Id == 1)
            {
                ValidationDictionary.AddError("Name", "Default vendor is locked for editing");
            }
            else
            {
                base.Update(vendor);
            }
        }
    }
}