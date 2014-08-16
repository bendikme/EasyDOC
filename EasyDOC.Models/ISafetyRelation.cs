using System;

namespace EasyDOC.Model
{
    public interface ISafetyRelation : IComparable<ISafetyRelation>, IIdentifyable
    {
        int SafetyId { get; set; }

        bool IncludeInManual { get; set; }

        SafetyRole Role { get; set; }
        string Location { get; set; }

        Safety Safety { get; set; }

        DateTime Created { get; set; }
        DateTime Edited { get; set; }
        int? CreatorId { get; set; }
        int? EditorId { get; set; }
        User Creator { get; set; }
        User Editor { get; set; }
    }
}