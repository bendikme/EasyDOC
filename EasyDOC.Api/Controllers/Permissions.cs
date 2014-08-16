using System;
using System.Linq;
using System.Web.Http;
using EasyDOC.DAL.DataAccess;
using WebMatrix.WebData;

namespace EasyDOC.Api.Controllers
{
    public class Permissions : ApiController
    {
        // GET api/permissions
        public string Get()
        {
            var uow = new UnitOfWork();
            var user = uow.UserRepository.GetByKey(WebSecurity.CurrentUserId);

            if (user == null)
            {
                throw new Exception("Unknown user id");
            }

            return Newtonsoft.Json.JsonConvert.SerializeObject(user.Roles.SelectMany(r => r.Role.Permissions).Select(p => new
            {
                Name = p.Permission.Name,
                Create = p.Create.ToString(),
                Read = p.Read.ToString(),
                Update = p.Update.ToString(),
                Delete = p.Delete.ToString(),
            }));
        }
    }
}
