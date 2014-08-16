using System;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public class DatabaseLog : DatabaseObject, IEntity, IHistoryEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string Content { get; set; }

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
            throw new NotImplementedException();
        }
    }
}