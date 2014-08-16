using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public interface ICustomerService : IGenericService<Customer>
    {
        void AddProject(Customer customer, int projectId);
        void RemoveProject(Customer customer, int projectId);

        void AddUser(Customer customer, int userId);
        void RemoveUser(Customer customer, int userId);
    }
}