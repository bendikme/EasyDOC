using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyDOC.Model
{
    public class ProjectComponent : IHistoryEntity, IIdentifyable
    {
        [Key]
        [Column(Order = 0)]
        public int ProjectId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int ComponentId { get; set; }

        [Required]
        public Decimal Count { get; set; }

        [Required]
        public Unit Unit { get; set; }

        public int SpareParts { get; set; }

        public bool IncludeInManual { get; set; }

        public int? FileId { get; set; }

        [MaxLength(1000)]
        public string Info { get; set; }

        public virtual File File { get; set; }
        public virtual Project Project { get; set; }
        public virtual AbstractComponent Component { get; set; }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public User Creator { get; set; }
        public User Editor { get; set; }

        public string GetId()
        {
            return string.Format("ProjectId:{0}:::ComponentId:{1}", ProjectId, ComponentId);
        }
    }
}