using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EasyDOC.Model
{
    public class Safety : DatabaseObject, IEntity, IHistoryEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string NameSe { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string DescriptionSe { get; set; }

        public virtual File Image { get; set; }
        public int? ImageId { get; set; }

        public int? VendorId { get; set; }

        public virtual ICollection<ComponentSafety> Components { get; set; }
        public virtual ICollection<ProjectSafety> Projects { get; set; }

        public override void RemoveAllReferences()
        {
            Components.Clear();
            Projects.Clear();
        }

        public override bool CanDelete()
        {
            return !Components.Any() && !Projects.Any();
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