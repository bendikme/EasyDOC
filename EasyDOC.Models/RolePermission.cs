using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyDOC.Model
{
    public class RolePermission : IHistoryEntity
    {
        [Key]
        [Column(Order = 0)]
        public int RoleId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int PermissionId { get; set; }

        [Required] public RoleScope Create { get; set; }
        [Required] public RoleScope Read { get; set; }
        [Required] public RoleScope Update { get; set; }
        [Required] public RoleScope Delete { get; set; }

        public virtual Role Role { get; set; }
        public virtual Permission Permission { get; set; }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }
    }

    public enum RoleScope
    {
        None,
        All,
        Owned,
        Managed
    }
}