using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EasyDOC.Model
{
    public abstract class AbstractComponent : DatabaseObject, IEntity, IHistoryEntity
    {
        protected AbstractComponent()
        {
            Projects = new HashSet<ProjectComponent>();
            Maintenances = new HashSet<ComponentMaintenance>();
            Files = new HashSet<ComponentFile>();
            Safeties = new HashSet<ComponentSafety>();
            Experiences = new HashSet<Experience>();
        }

        public override void RemoveAllReferences()
        {
            Projects.Clear();
            Maintenances.Clear();
            Files.Clear();
            Safeties.Clear();
        }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string DescriptionSe { get; set; }

        public int SpareParts { get; set; }

        public int? ImageId { get; set; }
        public int? CategoryId { get; set; }
        public int? VendorId { get; set; }

        public virtual File Image { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual Category Category { get; set; }

        public virtual ICollection<ComponentMaintenance> Maintenances { get; set; }
        public virtual ICollection<ComponentFile> Files { get; set; }

        public virtual ICollection<ProjectComponent> Projects { get; set; }
        public virtual ICollection<ComponentSafety> Safeties { get; set; }
        public virtual ICollection<Experience> Experiences { get; set; }

        [Timestamp]
        public byte[] Timestamp { get; set; }

        public override bool CanDelete()
        {
            return !Files.Any() && !Maintenances.Any() && !Projects.Any();
        }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }
    }
}