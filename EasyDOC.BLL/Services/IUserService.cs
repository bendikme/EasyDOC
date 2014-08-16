using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public interface IUserService : IGenericService<User>
    {
        User GetByUserName(string username);
    }
}