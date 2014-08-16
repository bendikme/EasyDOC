using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public class Task : DatabaseObject, IEntity, IHistoryEntity
    {
        public Task()
        {
            CheckPoints = new HashSet<TaskCheckPoint>();
            ChildTasks = new HashSet<Task>();
            Resources = new HashSet<EmployeeTask>();
            Predecessors = new HashSet<TaskLink>();
            Successors = new HashSet<TaskLink>();
        }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(7)]
        public string Color { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime ConstraintDate { get; set; }
        public string Duration { get; set; }

        public TaskConstraint ConstraintType { get; set; }
        public TaskCalendar CalendarType { get; set; }

        public string WbsCode { get; set; }

        public TaskState Status { get; set; }

        public int PercentComplete { get; set; }
        public int Priority { get; set; }

        public bool IsAutoScheduled { get; set; }
        public bool IsInactive { get; set; }
        public bool IsMilestone { get; set; }

        public int? ParentTaskId { get; set; }
        public virtual Task ParentTask { get; set; }

        public int? ProjectId { get; set; }
        public virtual Project Project { get; set; }

        public virtual ICollection<TaskCheckPoint> CheckPoints { get; set; }
        public virtual ICollection<Task> ChildTasks { get; set; }
        public virtual ICollection<EmployeeTask> Resources { get; set; }

        public virtual ICollection<TaskLink> Predecessors { get; set; }
        public virtual ICollection<TaskLink> Successors { get; set; }

        public override void RemoveAllReferences()
        {
            CheckPoints.Clear();
            ChildTasks.Clear();
            Resources.Clear();
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