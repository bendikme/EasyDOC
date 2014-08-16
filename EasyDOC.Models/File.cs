using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace EasyDOC.Model
{
    public class File : DatabaseObject, IEntity, IHistoryEntity
    {
        public File()
        {
            Chapters = new HashSet<ChapterFile>();
            Components = new HashSet<ComponentFile>();
            Projects = new HashSet<ProjectFile>();
            Maintenances = new HashSet<Maintenance>();
        }


        public int CatalogId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string FullName
        {
            get { return GetPath(); }
        }

        public int FileSize { get; set; }

        [Required]
        [MaxLength(10)]
        public string Type { get; set; }

        [MaxLength(100)]
        public string Link { get; set; }

        public bool IsOverwritable { get; set; }

        public virtual ICollection<ChapterFile> Chapters { get; set; }
        public virtual ICollection<ComponentFile> Components { get; set; }
        public virtual ICollection<ProjectFile> Projects { get; set; }
        public virtual ICollection<Maintenance> Maintenances { get; set; }

        [MaxLength(500)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public virtual Folder Folder { get; set; }

        [NotMapped]
        public string Path
        {
            get { return GetPath(); }
        }

        public string GetPath()
        {
            var url = new StringBuilder();
            url.Append("/" + Name + "." + Type);

            var parent = Folder;
            while (parent != null)
            {
                url.Insert(0, "/" + parent.Name);
                parent = parent.Parent;
            }

            return url.ToString();
        }

        public override void RemoveAllReferences()
        {
            throw new Exception("Cannot delete file " + Name + " because it is used by one or more entities.");
        }

        public override bool CanDelete()
        {
            return !Chapters.Any() && !Components.Any() && !Projects.Any() && !Maintenances.Any();
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