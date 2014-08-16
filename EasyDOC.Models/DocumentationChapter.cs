using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyDOC.Model
{
    public class DocumentationChapter : IHistoryEntity, IIdentifyable
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        public int DocumentationId { get; set; }

        [Key]
        [Column(Order = 1)]
        [Required]
        public int ChapterId { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(Order = 2)]
        [Required]
        public string UniqueKey { get; set; }

        [Display(Name = "Chapter number")]
        [Required]
        [MaxLength(10)]
        public string ChapterNumber { get; set; }

        public string Title { get; set; }
        public bool NewPage { get; set; }

        [RegularExpression(@"^\w+="".+""(;\w+="".+"")*$", ErrorMessage = "Wrong format")]
        public string Parameters { get; set; }

        public int? FileId { get; set; }
        public virtual File File { get; set; }

        public virtual AbstractChapter Chapter { get; set; }
        public virtual Documentation Documentation { get; set; }

        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }

        public string GetId()
        {
            return string.Format("DocumentationId:{0}:::ChapterId:{1}:::UniqueKey:{2}", DocumentationId, ChapterId, UniqueKey);
        }
    }
}