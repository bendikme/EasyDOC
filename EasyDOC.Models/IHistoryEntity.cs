using System;

namespace EasyDOC.Model
{
    public interface IHistoryEntity
    {
        DateTime Created { get; set; }
        DateTime Edited { get; set; }

        int? CreatorId { get; set; }
        int? EditorId { get; set; }

        User Creator { get; set; }
        User Editor { get; set; }
    }
}