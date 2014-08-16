using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyDOC.Model
{
    public class ComponentSafety : IHistoryEntity, ISafetyRelation, IIdentifyable
    {
        [Key]
        [Column(Order = 0)]
        public int ComponentId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int SafetyId { get; set; }

        public bool IncludeInManual { get; set; }

        public SafetyRole Role { get; set; }
        public string Location { get; set; }

        public virtual AbstractComponent Component { get; set; }
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
            return string.Format("ComponentId:{0}:::SafetyId:{1}", ComponentId, SafetyId);
        }
    }
}