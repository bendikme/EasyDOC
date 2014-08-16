using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public class Employee : DatabaseObject, IEntity, IHistoryEntity
    {
        public Employee()
        {
            CheckPoints = new HashSet<TaskCheckPoint>();
            Tasks = new HashSet<EmployeeTask>();
            ManagedProjects = new HashSet<Project>();
        }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Title { get; set; }

        public int? AccountId { get; set; }
        public virtual User Account { get; set; }

        public virtual ICollection<TaskCheckPoint> CheckPoints { get; set; }
        public virtual ICollection<Project> ManagedProjects { get; set; }
        public virtual ICollection<EmployeeTask> Tasks { get; set; }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public User Creator { get; set; }
        public User Editor { get; set; }

        public override void RemoveAllReferences()
        {
            throw new Exception("Cannot delete employee with bound tasks or projects");
        }
    }
}