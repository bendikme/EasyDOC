using System;

namespace EasyDOC.Model
{
    public abstract class SoftDeletableDatabaseObject : DatabaseObject, ISoftDeletable
    {
        public DateTime? Deleted { get; set; }

        public bool IsDeleted
        {
            get { return Deleted.HasValue; }
        }

        public void Restore()
        {
            Deleted = null;
        }
    }
}