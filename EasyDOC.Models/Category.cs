using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EasyDOC.Model
{
    public class Category : DatabaseObject, IEntity, IHistoryEntity
    {
        public Category()
        {
            Components = new HashSet<AbstractComponent>();
            SubCategories = new HashSet<Category>();
        }

        public int? ParentCategoryId { get; set; }

        public virtual Category ParentCategory { get; set; }
        public virtual ICollection<AbstractComponent> Components { get; set; }
        public virtual ICollection<Category> SubCategories { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(50)]
        public string NameSe { get; set; }

        public override void RemoveAllReferences()
        {
        }

        public override bool CanDelete()
        {
            return !Components.Any() && !SubCategories.Any();
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