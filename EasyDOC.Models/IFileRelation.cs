using System;
using System.ComponentModel.DataAnnotations;

namespace EasyDOC.Model
{
    public interface IFileRelation : IComparable<IFileRelation>, IIdentifyable
    {
        int FileId { get; set; }
        int? VendorId { get; set; }
        string Name { get; set; }

        [Display(Name = "Include in manual")]
        bool IncludeInManual { get; set; }

        [Display(Name = "Printed copy included")]
        bool IncludedPrintedVersion { get; set; }

        FileRole Role { get; set; }
        File File { get; set; }
        Vendor Vendor { get; set; }
    }
}