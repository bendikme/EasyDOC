using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class CustomerService : GenericService<Customer>, ICustomerService
    {
        public CustomerService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork)
            : base(validationDictionary, unitOfWork, unitOfWork.CustomerRepository) { }


        public void AddProject(Customer customer, int projectId)
        {
            var project = UnitOfWork.ProjectRepository.GetByKey(projectId);
            AddRelation(customer.Projects, customer, project);
        }

        public void RemoveProject(Customer customer, int projectId)
        {
            var project = customer.Projects.Single(c => c.Id == projectId);
            customer.Projects.Remove(project);
        }

        public void AddUser(Customer customer, int userId)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveUser(Customer customer, int userId)
        {
            throw new System.NotImplementedException();
        }
    }
}