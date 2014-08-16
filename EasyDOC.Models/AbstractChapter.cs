using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public abstract class AbstractChapter : DatabaseObject, IEntity, IHistoryEntity
    {
        protected AbstractChapter()
        {
            DocumentationChapters = new HashSet<DocumentationChapter>();
            Files = new HashSet<ChapterFile>();
        }

        public override void RemoveAllReferences()
        {
            DocumentationChapters.Clear();
            Files.Clear();
        }

        public virtual ICollection<DocumentationChapter> DocumentationChapters { get; set; }
        public virtual ICollection<ChapterFile> Files { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string NameSe { get; set; }

        public string Content { get; set; }
        public string ContentSe { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string DescriptionSe { get; set; }

        public int? ProjectId { get; set; }

        public virtual Project Project { get; set; }

        public override bool CanDelete()
        {
            return DocumentationChapters.Count == 0;
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