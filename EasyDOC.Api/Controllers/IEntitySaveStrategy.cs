using System.Data.Common;
using Breeze.ContextProvider;

namespace EasyDOC.Api.Controllers
{
    internal interface IEntitySaveStrategy
    {
        bool Execute(DbConnection connection, EntityInfo info);
    }
}