using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class CategoryService : GenericService<Category>
    {
        public CategoryService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork)
            : base(validationDictionary, unitOfWork, unitOfWork.CategoryRepository) { }

        public override void Update(Category category)
        {
            if (Repository.Get(t => t.Name == category.Name && t.Id != category.Id).Any())
            {
                ValidationDictionary.AddError("Name", "Unique name required");
            }
            else if (category.Id == 1)
            {
                ValidationDictionary.AddError("Name", "Default vendor is locked for editing");
            }
            else
            {
                base.Update(category);
            }
        }
    }
}