using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace EasyDOC.Model
{
    public class Customer : DatabaseObject, IEntity, IHistoryEntity
    {
        public Customer()
        {
            Projects = new HashSet<Project>();
        }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public virtual ICollection<Project> Projects { get; set; }

        [NotMapped]
        public int ProjectsCount { get; set; }

        public override void RemoveAllReferences() {}

        public override bool CanDelete()
        {
            return !Projects.Any();
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