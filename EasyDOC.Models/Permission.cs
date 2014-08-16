using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public class Permission : DatabaseObject, IEntity, IHistoryEntity
    {
        public Permission()
        {
            Roles = new HashSet<RolePermission>();
        }

        public virtual ICollection<RolePermission> Roles { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Info { get; set; }

        public override void RemoveAllReferences()
        {
            Roles.Clear();
        }

        public override bool CanDelete()
        {
            return false;
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