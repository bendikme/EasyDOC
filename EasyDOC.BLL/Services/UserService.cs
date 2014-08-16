using System.Linq;
using EasyDOC.DAL.DataAccess;
using EasyDOC.Model;

namespace EasyDOC.BLL.Services
{
    public class UserService : GenericService<User>, IUserService
    {
        public UserService(IValidationDictionary validationDictionary, IUnitOfWork unitOfWork)
            : base(validationDictionary, unitOfWork, unitOfWork.UserRepository)
        {
        }

        public User GetByUserName(string username)
        {
            return UnitOfWork.UserRepository.Get(user => user.Username == username).FirstOrDefault();
        }

        public override void Create(User user)
        {
            ValidateUserName(user.Username);
            ValidateEmail(user.Email);

            base.Create(user);
        }

        public override void Update(User item)
        {
            ValidateUserName(item.Username, item.Id);
            ValidateEmail(item.Email, item.Id);

            base.Update(item);
        }

        #region Validation helpers

        private void ValidateUserName(string name)
        {
            if (UnitOfWork.UserRepository.GetAllNoTracking().Any(u => u.Username == name))
            {
                ValidationDictionary.AddError("UserName", "This user name already exists");
            }
        }

        private void ValidateUserName(string name, int id)
        {
            if (UnitOfWork.UserRepository.GetAllNoTracking().Any(u => u.Username == name && u.Id != id))
            {
                ValidationDictionary.AddError("UserName", "This user name already exists");
            }
        }

        private void ValidateEmail(string email)
        {
            if (UnitOfWork.UserRepository.GetAllNoTracking().Any(u => u.Email == email))
            {
                ValidationDictionary.AddError("Email", "This email address is already registered to a user");
            }
        }

        private void ValidateEmail(string email, int id)
        {
            if (UnitOfWork.UserRepository.GetAllNoTracking().Any(u => u.Email == email && u.Id != id))
            {
                ValidationDictionary.AddError("Email", "This email address is already registered to a user");
            }
        }

        #endregion
    }
}
