using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public class Documentation : DatabaseObject, IEntity, IHistoryEntity
    {
        public Documentation()
        {
            DocumentationChapters = new HashSet<DocumentationChapter>();
        }

        public int? ProjectId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string NameSe { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public int? ImageId { get; set; }
        public virtual File Image { get; set; }

        public virtual Project Project { get; set; }
        public virtual ICollection<DocumentationChapter> DocumentationChapters { get; set; }

        public override void RemoveAllReferences()
        {
            DocumentationChapters.Clear();
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