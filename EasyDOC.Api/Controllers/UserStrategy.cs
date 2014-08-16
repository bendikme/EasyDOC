using System.Data.Common;
using System.Web.Security;
using Breeze.ContextProvider;
using EasyDOC.Model;
using WebMatrix.WebData;

namespace EasyDOC.Api.Controllers
{
    internal class UserStrategyBefore : IEntitySaveStrategy
    {
        public bool Execute(DbConnection connection, EntityInfo info)
        {
            var user = info.Entity as User;
            var username = info.OriginalValuesMap.ContainsKey("Username") ? info.OriginalValuesMap["Username"].ToString() : user.Username;

            if (info.EntityState == EntityState.Modified || info.EntityState == EntityState.Deleted)
            {
                ((SimpleMembershipProvider)Membership.Provider).DeleteAccount(username);
            }

            return true;
        }
    }

    internal class UserStrategyAfter : IEntitySaveStrategy
    {
        public bool Execute(DbConnection connection, EntityInfo info)
        {
            var user = info.Entity as User;

            if (info.EntityState == EntityState.Added || info.EntityState == EntityState.Modified)
            {
                WebSecurity.CreateAccount(user.Username, "awio348vnadkjf");
            }

            return true;
        }
    }
}