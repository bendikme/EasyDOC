using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace EasyDOC.Model
{
    public class Folder : DatabaseObject, IEntity, IHistoryEntity
    {
        public Folder()
        {
            Catalogs = new HashSet<Folder>();
            Files = new HashSet<File>();
        }

        public int? ParentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(500)]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public virtual Folder Parent { get; set; }
        public virtual ICollection<Folder> Catalogs { get; set; }
        public virtual ICollection<File> Files { get; set; }

        public string GetPath()
        {
            var url = new StringBuilder();
            url.Append("/" + Name);

            var parent = Parent;
            while (parent != null)
            {
                url.Insert(0, "/" + parent.Name);
                parent = parent.Parent;
            }

            return url.ToString();
        }

        public string GetWindowsPath()
        {
            return GetPath().Replace("/", "\\").Substring(1);
        }

        public override void RemoveAllReferences() {}

        public override bool CanDelete()
        {
            return !(Catalogs.Any() || Files.Any());
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