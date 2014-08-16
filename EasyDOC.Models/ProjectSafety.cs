using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyDOC.Model
{
    public class ProjectSafety : IHistoryEntity, ISafetyRelation
    {
        [Key]
        [Column(Order = 0)]
        public int ProjectId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int SafetyId { get; set; }

        [DefaultValue(true)]
        public bool IncludeInManual { get; set; }

        public SafetyRole Role { get; set; }

        [MaxLength(1000)]
        public string Location { get; set; }

        public virtual Project Project { get; set; }
        public virtual Safety Safety { get; set; }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }

        public int CompareTo(ISafetyRelation other)
        {
            return String.Compare(Safety.Name, other.Safety.Name, StringComparison.Ordinal);
        }

        public string GetId()
        {
            return string.Format("ProjectId:{0}:::SafetyId:{1}", ProjectId, SafetyId);
        }
    }
}