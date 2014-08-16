
namespace EasyDOC.Model
{
    public abstract class DatabaseObject : IIdentifyable
    {
        public int Id { get; set; }

        public virtual string GetTypeName()
        {
            return GetType().Name.ToLowerInvariant();
        }

        public abstract void RemoveAllReferences();

        public virtual bool CanDelete()
        {
            return true;
        }

        public string GetId()
        {
            return string.Format("Id:{0}", Id);
        }
    }
}