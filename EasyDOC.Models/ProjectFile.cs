using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyDOC.Model
{
    public class ProjectFile : IFileRelation, IHistoryEntity
    {
        [Key]
        [Column(Order = 0)]
        public int ProjectId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int FileId { get; set; }

        public FileRole Role { get; set; }

        public int? VendorId { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [Display(Name = "Include in manual")]
        public bool IncludeInManual { get; set; }

        [Display(Name = "Printed version included")]
        public bool IncludedPrintedVersion { get; set; }

        public virtual Vendor Vendor { get; set; }
        public virtual Project Project { get; set; }
        public virtual File File { get; set; }

        public int CompareTo(IFileRelation other)
        {
            return String.Compare(Name, other.Name, StringComparison.Ordinal);
        }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }
        
        public string GetId()
        {
            return string.Format("ProjectId:{0}:::FileId:{1}", ProjectId, FileId);
        }
    }
}