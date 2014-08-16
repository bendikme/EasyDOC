using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public class Project : DatabaseObject, IEntity, IHistoryEntity
    {
        public Project()
        {
            Chapters = new HashSet<AbstractChapter>();
            Components = new HashSet<ProjectComponent>();
            Documentations = new HashSet<Documentation>();
            Experiences = new HashSet<Experience>();
            Files = new HashSet<ProjectFile>();
            Maintenances = new HashSet<ProjectMaintenance>();
            OrderConfirmations = new HashSet<OrderConfirmation>();
            Safeties = new HashSet<ProjectSafety>();
            ServiceLogs = new HashSet<ServiceLog>();
            SubProjects = new HashSet<Project>();
        }

        public int? CustomerId { get; set; }
        public virtual Customer Customer { get; set; }

        public int? ProjectManagerId { get; set; }
        public virtual Employee ProjectManager { get; set; }

        public int? MasterProjectId { get; set; }
        public virtual Project MasterProject { get; set; }

        [Required]
        [Display(Name = "Project number")]
        [MaxLength(50)]
        public string ProjectNumber { get; set; }

        public ProjectStatus Status { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        public virtual ICollection<AbstractChapter> Chapters { get; set; }
        public virtual ICollection<ProjectComponent> Components { get; set; }
        public virtual ICollection<Documentation> Documentations { get; set; }
        public virtual ICollection<Experience> Experiences { get; set; }
        public virtual ICollection<ProjectFile> Files { get; set; }
        public virtual ICollection<ProjectMaintenance> Maintenances { get; set; }
        public virtual ICollection<OrderConfirmation> OrderConfirmations { get; set; }
        public virtual ICollection<ProjectSafety> Safeties { get; set; }
        public virtual ICollection<ServiceLog> ServiceLogs { get; set; }
        public virtual ICollection<Project> SubProjects { get; set; }

        public string ProjectNumberAndName
        {
            get { return ProjectNumber + ": " + Name; }
        }

        public override void RemoveAllReferences()
        {
            Chapters.Clear();
            Components.Clear();
            Documentations.Clear();
            Experiences.Clear();
            Files.Clear();
            Maintenances.Clear();
            OrderConfirmations.Clear();
            Safeties.Clear();
            ServiceLogs.Clear();
            SubProjects.Clear();
        }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }
    }
}