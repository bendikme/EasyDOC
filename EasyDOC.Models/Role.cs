using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public class Role : DatabaseObject, IEntity, IHistoryEntity
    {
        public Role()
        {
            Permissions = new HashSet<RolePermission>();
            Users = new HashSet<UserRole>();
        }

        public virtual ICollection<RolePermission> Permissions { get; set; }
        public virtual ICollection<UserRole> Users { get; set; }

        [Required]
        public string Name { get; set; }
        public string Info { get; set; }

        public override void RemoveAllReferences()
        {
            Permissions.Clear();
            Users.Clear();
        }

        public override bool CanDelete()
        {
            return Permissions.Count == 0 && Users.Count == 0;
        }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }
    }
}