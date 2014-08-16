using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyDOC.Model
{
    public class TaskLink : IHistoryEntity
    {
        [Key, Column(Order = 0)]
        public int FromId { get; set; }
        public virtual Task From { get; set; }

        [Key, Column(Order = 1)]
        public int ToId { get; set; }
        public virtual Task To { get; set; }

        [Key, Column(Order = 2)]
        public TaskLinkType Type { get; set; }

        public string Lag { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }
        public string Name { get; set; }
    }
}