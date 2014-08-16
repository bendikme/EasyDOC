using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public class TaskCheckPoint : DatabaseObject, IEntity, IHistoryEntity
    {
        [Required]
        public string Name { get; set; }
        public string Info { get; set; }

        public bool Completed { get; set; }
        public DateTime Deadline { get; set; }

        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int? TaskId { get; set; }
        public Task Task { get; set; }

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