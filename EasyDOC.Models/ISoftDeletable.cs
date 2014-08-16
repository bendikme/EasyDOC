using System;

namespace EasyDOC.Model
{
    public interface ISoftDeletable
    {
        DateTime? Deleted { get; set; }
        bool IsDeleted { get; }
        void Restore();
    }
}