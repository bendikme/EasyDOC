using System;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public class Experience : DatabaseObject, IEntity, IHistoryEntity
    {
        public int? ProjectId { get; set; }
        public int? ComponentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string Content { get; set; }

        [MaxLength(500)]
        public string Tags { get; set; }

        public virtual Project Project { get; set; }
        public virtual AbstractComponent Component { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }

        public override void RemoveAllReferences()
        {
            
        }
    }
}