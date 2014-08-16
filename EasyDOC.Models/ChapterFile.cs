using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyDOC.Model
{
    public class ChapterFile : IHistoryEntity, IIdentifyable
    {
        [Key]
        [Column(Order = 0)]
        public int ChapterId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int FileId { get; set; }

        public virtual AbstractChapter Chapter { get; set; }
        public virtual File File { get; set; }
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public int? CreatorId { get; set; }
        public int? EditorId { get; set; }
        public virtual User Creator { get; set; }
        public virtual User Editor { get; set; }

        public string GetId()
        {
            return string.Format("ChapterId:{0}:::FileId:{1}", ChapterId, FileId);
        }
    }
}